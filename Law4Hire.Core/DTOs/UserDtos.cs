namespace Law4Hire.Core.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? MiddleName, 
    string? LastName,
    string? PhoneNumber,
    string PreferredLanguage,
    DateTime CreatedAt,
    bool IsActive,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    DateTime? DateOfBirth,
    string? Category,
    Guid? CitizenshipCountryId,
    string? CitizenshipCountryName,
    string? MaritalStatus
);

public record CreateUserDto(
    string Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? PhoneNumber,
    string PreferredLanguage,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    DateTime? DateOfBirth
);

public record UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Guid? CitizenshipCountryId { get; set; }
    public string? MaritalStatus { get; set; }
}

public record UpdateCategoryDto
{
    public string? Category { get; set; }
}

public record UserProfileSummaryDto(
    Guid UserId,
    string UserName,
    string Email,
    SelectedPackageDto? SelectedPackage,
    AssignedVisaTypeDto? AssignedVisaType
);

public record SelectedPackageDto(
    int Id,
    string Name,
    string Description
);

public record AssignedVisaTypeDto(
    Guid Id,
    string Name,
    string Category
);
