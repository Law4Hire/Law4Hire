using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IServicePackageRepository
{
    Task<IEnumerable<ServicePackage>> GetAllActiveAsync();
    Task<ServicePackage?> GetByIdAsync(int id);
    Task<IEnumerable<ServicePackage>> GetByVisaTypeNameAsync(string visaTypeName);
    Task<ServicePackage> CreateAsync(ServicePackage package);
    Task<ServicePackage> UpdateAsync(ServicePackage package);
}