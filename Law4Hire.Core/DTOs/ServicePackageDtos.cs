using Law4Hire.Core.Enums;

namespace Law4Hire.Core.DTOs;

public record ServicePackageDto(
    int Id,
    string Name,
    string Description,
    PackageType Type,
    decimal BasePrice,
    decimal L4HLLCFee,
    bool HasMoneyBackGuarantee,
    bool IsActive,
    Guid VisaTypeId,
    string? VisaTypeName = null
);

public record CreateServicePackageDto(
    string Name,
    string Description,
    PackageType Type,
    decimal BasePrice,
    decimal L4HLLCFee,
    bool HasMoneyBackGuarantee,
    Guid VisaTypeId
);

public class UpdateServicePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PackageType Type { get; set; }
    public decimal BasePrice { get; set; }
    public decimal L4HLLCFee { get; set; }
    public bool HasMoneyBackGuarantee { get; set; }
    public bool IsActive { get; set; }
}

public record VisaTypePackageDto(
    Guid VisaTypeId,
    string VisaTypeName,
    string? VisaTypeDescription,
    string VisaTypeStatus,
    List<ServicePackageDto> Packages,
    ServicePackageDto? SelectedPackage = null
);