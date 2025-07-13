using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IVisaTypeQuestionRepository
{
    Task<IEnumerable<VisaTypeQuestion>> GetByVisaTypeIdAsync(Guid visaTypeId);
    Task UpsertRangeAsync(IEnumerable<VisaTypeQuestion> questions);
}
