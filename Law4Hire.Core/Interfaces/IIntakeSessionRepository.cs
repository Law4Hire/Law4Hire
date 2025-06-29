using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IIntakeSessionRepository
{
    Task<IntakeSession?> GetByIdAsync(Guid id);
    Task<IEnumerable<IntakeSession>> GetByUserIdAsync(Guid userId);
    Task<IntakeSession> CreateAsync(IntakeSession session);
    Task<IntakeSession> UpdateAsync(IntakeSession session);
}