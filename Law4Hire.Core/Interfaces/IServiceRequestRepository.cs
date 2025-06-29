using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IServiceRequestRepository
{
    Task<ServiceRequest?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceRequest>> GetByUserIdAsync(Guid userId);
    Task<ServiceRequest> CreateAsync(ServiceRequest request);
    Task<ServiceRequest> UpdateAsync(ServiceRequest request);
}