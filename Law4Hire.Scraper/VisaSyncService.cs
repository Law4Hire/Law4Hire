using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Law4Hire.Scraper
{
    public class VisaSyncService : IVisaSyncService
    {
        private readonly Law4HireDbContext _db;
        private readonly IBruceOpenAIAgent _bruce;
        private readonly IScrapeLogRepository _log;
        private readonly ILogger<VisaSyncService> _logger;

        public VisaSyncService(
            Law4HireDbContext db,
            IBruceOpenAIAgent bruce,
            IScrapeLogRepository log,
            ILogger<VisaSyncService> logger)
        {
            _db = db;
            _bruce = bruce;
            _log = log;
            _logger = logger;
        }

        public async Task SyncCategoriesAndTypesAsync(CancellationToken token)
        {
            var categories = await _db.VisaCategories.ToListAsync(token);

            foreach (var category in categories)
            {
                _logger.LogInformation("🔄 Syncing category: {Category}", category.Name);

                var latestSubs = await _bruce.GetSubCategoriesAsync(category.Name);
                var currentSubs = await _db.VisaSubCategories
                    .Where(s => s.CategoryId == category.Id)
                    .ToListAsync(token);

                // Add new sub-categories
                foreach (var sub in latestSubs)
                {
                    if (!currentSubs.Any(c => c.Name.Equals(sub, StringComparison.OrdinalIgnoreCase)))
                    {
                        bool isValid = await _bruce.ValidateSubCategoryAsync(category.Name, sub);

                        if (isValid)
                        {
                            var newSub = new VisaSubCategory
                            {
                                Id = Guid.NewGuid(),
                                Name = sub,
                                CategoryId = category.Id,
                                Status = "Validated",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _db.VisaSubCategories.Add(newSub);
                            await _log.AddAsync(new ScrapeLog
                            {
                                Id = Guid.NewGuid(),
                                Timestamp = DateTime.UtcNow,
                                Action = "SubCategory Added",
                                EntityAffected = $"{category.Name} - {sub}",
                                Notes = "New sub-category"
                            });
                        }
                    }
                }

                // Remove deleted sub-categories (after validation)
                foreach (var existing in currentSubs)
                {
                    if (!latestSubs.Any(s => s.Equals(existing.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        bool stillValid = await _bruce.ValidateSubCategoryAsync(category.Name, existing.Name);

                        if (!stillValid)
                        {
                            existing.Status = "Removed";
                            existing.UpdatedAt = DateTime.UtcNow;

                            await _log.AddAsync(new ScrapeLog
                            {
                                Id = Guid.NewGuid(),
                                Timestamp = DateTime.UtcNow,
                                Action = "SubCategory Removed",
                                EntityAffected = $"{category.Name} - {existing.Name}",
                                Notes = "Bruce marked as invalid"
                            });
                        }
                    }
                }

                await _db.SaveChangesAsync(token);

                // Only keep valid subcategories
                var validSubNames = await _db.VisaSubCategories
                    .Where(s => s.CategoryId == category.Id && s.Status == "Validated")
                    .Select(s => s.Name)
                    .ToListAsync(token);

                var visaTypes = await _bruce.GetVisaTypesAsync(category.Name, validSubNames);

                // Validate visa types: only update if different
                var currentTypes = category.VisaTypesJson != null
                    ? JsonSerializer.Deserialize<List<string>>(category.VisaTypesJson) ?? []
                    : [];

                bool changed = !visaTypes.SequenceEqual(currentTypes, StringComparer.OrdinalIgnoreCase);

                if (changed)
                {
                    category.VisaTypesJson = JsonSerializer.Serialize(visaTypes);
                    category.UpdatedAt = DateTime.UtcNow;

                    await _log.AddAsync(new ScrapeLog
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        Action = "VisaTypes Updated",
                        EntityAffected = category.Name,
                        Notes = $"Visa types updated from Bruce."
                    });

                    await _db.SaveChangesAsync(token);
                }
                else
                {
                    await _log.AddAsync(new ScrapeLog
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        Action = "VisaTypes Skipped",
                        EntityAffected = category.Name,
                        Notes = "No changes in visa types"
                    });
                }
            }
        }
    }
}
