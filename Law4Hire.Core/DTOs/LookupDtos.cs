namespace Law4Hire.Core.DTOs;

public record CountryDto(
    Guid Id,
    string Name,
    string CountryCode,
    string CountryCode2,
    bool IsUNRecognized,
    int SortOrder
);

public record USStateDto(
    Guid Id,
    string Name,
    string StateCode,
    string StateName,
    bool IsTerritory,
    int SortOrder
);

public record VisaCategoryDto(
    Guid Id,
    string Name,
    string Description,
    string IconName,
    bool IsActive
);