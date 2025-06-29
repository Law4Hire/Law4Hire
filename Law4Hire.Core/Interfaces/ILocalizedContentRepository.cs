using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface ILocalizedContentRepository
{
    Task<LocalizedContent?> GetContentAsync(string key, string language);
    Task<IEnumerable<LocalizedContent>> GetAllForLanguageAsync(string language);
    Task<LocalizedContent> CreateOrUpdateAsync(LocalizedContent content);
}