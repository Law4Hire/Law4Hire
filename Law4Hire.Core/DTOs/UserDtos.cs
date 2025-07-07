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
    DateTime? DateOfBirth
    
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
