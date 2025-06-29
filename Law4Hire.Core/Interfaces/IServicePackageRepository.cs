using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IServicePackageRepository
{
    Task<IEnumerable<ServicePackage>> GetAllActiveAsync();
    Task<ServicePackage?> GetByIdAsync(int id);
    Task<ServicePackage> CreateAsync(ServicePackage package);
    Task<ServicePackage> UpdateAsync(ServicePackage package);
}