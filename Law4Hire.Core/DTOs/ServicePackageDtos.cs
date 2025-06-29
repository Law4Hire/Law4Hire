using Law4Hire.Core.Enums;

namespace Law4Hire.Core.DTOs;

public record ServicePackageDto(
    int Id,
    string Name,
    string Description,
    PackageType Type,
    decimal BasePrice,
    bool HasMoneyBackGuarantee,
    bool IsActive
);