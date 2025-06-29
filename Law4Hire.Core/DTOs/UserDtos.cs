namespace Law4Hire.Core.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string PreferredLanguage,
    DateTime CreatedAt,
    bool IsActive
);

public record CreateUserDto(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string PreferredLanguage
);