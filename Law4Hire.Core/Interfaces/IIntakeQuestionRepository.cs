using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IIntakeQuestionRepository
{
    Task<IEnumerable<IntakeQuestion>> GetByCategoryAsync(string category);
}
