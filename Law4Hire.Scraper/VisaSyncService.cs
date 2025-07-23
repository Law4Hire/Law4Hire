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

                // Step 1: Get and validate sub-categories
                var validSubNames = await ProcessSubCategoriesAsync(category, token);

                // Step 2: Get visa types from Bruce
                var latestVisaTypes = await _bruce.GetVisaTypesAsync(category.Name, validSubNames);

                // Step 3: Update BaseVisaTypes table
                await UpdateBaseVisaTypesAsync(category, validSubNames, latestVisaTypes, token);
            }
        }

        private async Task<List<string>> ProcessSubCategoriesAsync(VisaCategory category, CancellationToken token)
        {
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
                            Notes = "New sub-category validated by Bruce"
                        });
                    }
                }
            }

            // Mark removed sub-categories
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

            // Return only valid subcategory names
            return await _db.VisaSubCategories
                .Where(s => s.CategoryId == category.Id && s.Status == "Validated")
                .Select(s => s.Name)
                .ToListAsync(token);
        }

        private async Task UpdateBaseVisaTypesAsync(
            VisaCategory category,
            List<string> subCategories,
            List<string> latestVisaTypes,
            CancellationToken token)
        {
            var existingBaseTypes = await _db.BaseVisaTypes
                .Where(bvt => bvt.CategoryId == category.Id)
                .ToListAsync(token);

            var subCategoriesJson = JsonSerializer.Serialize(subCategories);
            var addedCount = 0;
            var updatedCount = 0;

            // Add or update visa types
            foreach (var visaTypeName in latestVisaTypes.Where(vt => !string.IsNullOrWhiteSpace(vt)))
            {
                var existing = existingBaseTypes.FirstOrDefault(e =>
                    e.Name.Equals(visaTypeName, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    // Add new base visa type
                    var newBaseType = new BaseVisaType
                    {
                        Id = Guid.NewGuid(),
                        Name = visaTypeName.Trim(),
                        CategoryId = category.Id,
                        RelatedSubCategories = subCategoriesJson,
                        Status = "Active",
                        DiscoveredAt = DateTime.UtcNow,
                        LastConfirmedAt = DateTime.UtcNow,
                        ConfidenceScore = 1.0m
                    };

                    _db.BaseVisaTypes.Add(newBaseType);
                    addedCount++;
                }
                else
                {
                    // Update existing
                    existing.RelatedSubCategories = subCategoriesJson;
                    existing.LastConfirmedAt = DateTime.UtcNow;
                    existing.Status = "Active"; // Reactivate if it was deprecated
                    updatedCount++;
                }
            }

            // Mark missing visa types as deprecated
            var currentVisaTypeNames = latestVisaTypes
                .Where(vt => !string.IsNullOrWhiteSpace(vt))
                .Select(vt => vt.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var existing in existingBaseTypes.Where(e => e.Status == "Active"))
            {
                if (!currentVisaTypeNames.Contains(existing.Name))
                {
                    existing.Status = "Deprecated";
                    existing.LastConfirmedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync(token);

            // Log the results
            await _log.AddAsync(new ScrapeLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Action = "BaseVisaTypes Updated",
                EntityAffected = category.Name,
                Notes = $"Added: {addedCount}, Updated: {updatedCount}, Total: {latestVisaTypes.Count}"
            });

            _logger.LogInformation("✅ {Category}: Added {Added}, Updated {Updated} base visa types",
                category.Name, addedCount, updatedCount);
        }
    }
}