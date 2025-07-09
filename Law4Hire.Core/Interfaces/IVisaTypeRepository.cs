using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IVisaTypeRepository
{
    Task<IEnumerable<VisaType>> GetAllAsync();
    Task<IEnumerable<VisaType>> GetByCategoryAsync(string category);
    Task<VisaType?> GetByIdAsync(Guid id);
}
