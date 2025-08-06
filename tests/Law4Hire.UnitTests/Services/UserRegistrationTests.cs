using Law4Hire.Core.Entities;
using Law4Hire.Core.DTOs;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Law4Hire.UnitTests.Services;

[TestFixture]
public class UserRegistrationTests
{
    private Law4HireDbContext _context = null!;
    private UserManager<User> _userManager = null!;
    private RoleManager<IdentityRole<Guid>> _roleManager = null!;
    private ServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        var options = new DbContextOptionsBuilder<Law4HireDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new Law4HireDbContext(options);
        services.AddSingleton(_context);

        // Add Identity services
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<Law4HireDbContext>()
        .AddDefaultTokenProviders();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        
        // Ensure database is created
        _context.Database.EnsureCreated();
        
        _userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Create roles
        CreateRolesAsync().Wait();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task CreateRolesAsync()
    {
        var roles = new[] { "User", "Admin", "LegalProfessional" };
        
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }
        }
    }

    [Test]
    public async Task CreateUserFromUI_ValidData_CreatesUserSuccessfully()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        var testUser = new User
        {
            Email = $"testuser{randomId}@testing.com",
            UserName = $"testuser{randomId}@testing.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890",
            City = "Test City",
            State = "CA",
            Country = "United States",
            Category = "Work",
            DateOfBirth = new DateTime(1990, 1, 1),
            CitizenshipCountryId = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var password = "SecureTest123!";

        // Act
        var result = await _userManager.CreateAsync(testUser, password);

        // Assert
        Assert.That(result.Succeeded, Is.True, $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        
        var createdUser = await _userManager.FindByEmailAsync(testUser.Email);
        Assert.That(createdUser, Is.Not.Null, "Created user should be retrievable");
        Assert.That(createdUser.Email, Is.EqualTo(testUser.Email), "Email should match");
        Assert.That(createdUser.FirstName, Is.EqualTo("Test"), "First name should match");
        Assert.That(createdUser.LastName, Is.EqualTo("User"), "Last name should match");
        Assert.That(createdUser.Category, Is.EqualTo("Work"), "Category should match");
    }

    [Test]
    public async Task CreateUserFromUI_WithUserRole_AssignsRoleCorrectly()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        var testUser = new User
        {
            Email = $"userrole{randomId}@testing.com",
            UserName = $"userrole{randomId}@testing.com",
            FirstName = "Role",
            LastName = "Test",
            Category = "Family"
        };

        var password = "SecureTest123!";

        // Act
        var createResult = await _userManager.CreateAsync(testUser, password);
        var roleResult = await _userManager.AddToRoleAsync(testUser, "User");

        // Assert
        Assert.That(createResult.Succeeded, Is.True, "User creation should succeed");
        Assert.That(roleResult.Succeeded, Is.True, "Role assignment should succeed");
        
        var userRoles = await _userManager.GetRolesAsync(testUser);
        Assert.That(userRoles, Contains.Item("User"), "User should have User role");
    }

    [Test]
    public async Task CreateUserFromUI_PasswordValidation_EnforcesPasswordPolicy()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        var testUser = new User
        {
            Email = $"weakpass{randomId}@testing.com",
            UserName = $"weakpass{randomId}@testing.com",
            FirstName = "Weak",
            LastName = "Password"
        };

        var weakPassword = "weak"; // This should fail validation

        // Act
        var result = await _userManager.CreateAsync(testUser, weakPassword);

        // Assert
        Assert.That(result.Succeeded, Is.False, "Weak password should not be accepted");
        Assert.That(result.Errors.Count(), Is.GreaterThan(0), "Should have password validation errors");
        
        var hasPasswordErrors = result.Errors.Any(e => 
            e.Code.Contains("Password") || 
            e.Description.Contains("password", StringComparison.OrdinalIgnoreCase));
        Assert.That(hasPasswordErrors, Is.True, "Should have password-related errors");
    }

    [Test]
    public async Task CreateUserFromUI_ValidPassword_MeetsAllRequirements()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        var testUser = new User
        {
            Email = $"strongpass{randomId}@testing.com",
            UserName = $"strongpass{randomId}@testing.com",
            FirstName = "Strong",
            LastName = "Password"
        };

        var strongPassword = "SecureTest123!"; // Meets all requirements

        // Act
        var result = await _userManager.CreateAsync(testUser, strongPassword);

        // Assert
        Assert.That(result.Succeeded, Is.True, $"Strong password should be accepted: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        
        // Verify password can be verified
        var createdUser = await _userManager.FindByEmailAsync(testUser.Email);
        var passwordCheck = await _userManager.CheckPasswordAsync(createdUser!, strongPassword);
        Assert.That(passwordCheck, Is.True, "Password should be verifiable");
    }

    [Test]
    public async Task CreateUserFromUI_DuplicateEmail_ShouldFail()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid().ToString("N")[..8]}@testing.com";
        
        var firstUser = new User
        {
            Email = email,
            UserName = email,
            FirstName = "First",
            LastName = "User"
        };

        var secondUser = new User
        {
            Email = email,
            UserName = email,
            FirstName = "Second",
            LastName = "User"
        };

        var password = "SecureTest123!";

        // Act
        var firstResult = await _userManager.CreateAsync(firstUser, password);
        var secondResult = await _userManager.CreateAsync(secondUser, password);

        // Assert
        Assert.That(firstResult.Succeeded, Is.True, "First user creation should succeed");
        Assert.That(secondResult.Succeeded, Is.False, "Second user with same email should fail");
        
        var hasDuplicateError = secondResult.Errors.Any(e => 
            e.Description.Contains("already taken", StringComparison.OrdinalIgnoreCase) ||
            e.Description.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
        Assert.That(hasDuplicateError, Is.True, "Should have duplicate email error");
    }

    [Test]
    public async Task CreateUserFromUI_WithProfile_CreatesExtendedProfile()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        var testUser = new User
        {
            Email = $"profile{randomId}@testing.com",
            UserName = $"profile{randomId}@testing.com",
            FirstName = "Profile",
            LastName = "Test",
            DateOfBirth = new DateTime(1985, 6, 15),
            Category = "Study"
        };

        var password = "SecureTest123!";

        // Act
        var userResult = await _userManager.CreateAsync(testUser, password);
        
        // Create extended profile
        var userProfile = new UserProfile
        {
            UserId = testUser.Id,
            DateOfBirth = testUser.DateOfBirth,
            MaritalStatus = "Single",
            EducationLevel = "Bachelor's Degree",
            HasJobOffer = false,
            HasRelativesInUS = true,
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false,
            Notes = "Test profile for unit testing"
        };

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // Assert
        Assert.That(userResult.Succeeded, Is.True, "User creation should succeed");
        
        var createdProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == testUser.Id);
        Assert.That(createdProfile, Is.Not.Null, "User profile should be created");
        Assert.That(createdProfile.MaritalStatus, Is.EqualTo("Single"), "Marital status should match");
        Assert.That(createdProfile.EducationLevel, Is.EqualTo("Bachelor's Degree"), "Education level should match");
        Assert.That(createdProfile.HasRelativesInUS, Is.True, "Has relatives flag should match");
    }

    [Test]
    public async Task CreateUserFromUI_CategoryValidation_AcceptsValidCategories()
    {
        // Arrange
        var validCategories = new[] { "Visit", "Work", "Study", "Family", "Investment", "Asylum", "Immigrate" };
        var password = "SecureTest123!";
        var createdUsers = new List<User>();

        // Act & Assert
        foreach (var category in validCategories)
        {
            var randomId = Guid.NewGuid().ToString("N")[..8];
            var testUser = new User
            {
                Email = $"{category.ToLower()}{randomId}@testing.com",
                UserName = $"{category.ToLower()}{randomId}@testing.com",
                FirstName = category,
                LastName = "User",
                Category = category
            };

            var result = await _userManager.CreateAsync(testUser, password);
            
            Assert.That(result.Succeeded, Is.True, $"User creation should succeed for category: {category}");
            createdUsers.Add(testUser);
        }

        Assert.That(createdUsers.Count, Is.EqualTo(validCategories.Length), "Should create users for all valid categories");
    }

    [Test]
    public async Task CreateUserFromUI_TestingDomainEmail_IsValid()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        var testUser = new User
        {
            Email = $"user{randomId}@testing.com",
            UserName = $"user{randomId}@testing.com",
            FirstName = "Testing",
            LastName = "Domain"
        };

        var password = "SecureTest123!";

        // Act
        var result = await _userManager.CreateAsync(testUser, password);

        // Assert
        Assert.That(result.Succeeded, Is.True, "Testing.com domain should be valid for testing");
        Assert.That(testUser.Email, Does.EndWith("@testing.com"), "Email should use testing.com domain");
        
        var createdUser = await _userManager.FindByEmailAsync(testUser.Email);
        Assert.That(createdUser, Is.Not.Null, "User with testing.com email should be retrievable");
    }

    [Test]
    public void UserRegistrationDto_ValidationAttributes_ValidateCorrectly()
    {
        // Arrange
        var validDto = new UserRegistrationDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@testing.com",
            PhoneNumber = "555-0123",
            Password = "SecureTest123!",
            Address1 = "123 Test St",
            City = "Test City",
            State = "CA",
            Country = "United States",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            MaritalStatus = "Single",
            EducationLevel = "Bachelor's",
            HasRelativesInUS = false,
            HasJobOffer = false,
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false,
            ImmigrationGoal = "Work"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(validDto, null, null);
        var isValid = Validator.TryValidateObject(validDto, context, validationResults, true);

        // Assert
        Assert.That(isValid, Is.True, $"Valid DTO should pass validation. Errors: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
        Assert.That(validationResults.Count, Is.EqualTo(0), "Valid DTO should have no validation errors");
    }

    [Test]
    public void UserRegistrationDto_EmptyRequiredFields_FailsValidation()
    {
        // Arrange
        var invalidDto = new UserRegistrationDto
        {
            // Missing required fields: FirstName, LastName, Email, Password
            PhoneNumber = "555-0123",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(invalidDto, null, null);
        var isValid = Validator.TryValidateObject(invalidDto, context, validationResults, true);

        // Assert
        Assert.That(isValid, Is.False, "DTO with missing required fields should fail validation");
        Assert.That(validationResults.Count, Is.GreaterThan(0), "Should have validation errors for missing fields");
    }

    [Test]
    public async Task CreateUserFromUI_FullRegistrationDto_CreatesCompleteUser()
    {
        // Arrange
        var randomId = Guid.NewGuid().ToString("N")[..8];
        
        // Create a mock country for testing
        var testCountry = new Country
        {
            Id = Guid.NewGuid(),
            Name = "Test Country",
            CountryCode = "TEST",
            CountryCode2 = "TE",
            IsUNRecognized = true,
            SortOrder = 999
        };
        _context.Countries.Add(testCountry);
        await _context.SaveChangesAsync();

        var registrationDto = new UserRegistrationDto
        {
            FirstName = "Complete",
            LastName = "Test",
            Email = $"complete{randomId}@testing.com",
            PhoneNumber = "555-0123",
            Password = "SecureTest123!",
            MiddleName = "Middle",
            Address1 = "123 Complete St",
            Address2 = "Apt 1",
            City = "Complete City",
            State = "CA",
            Country = "United States",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            CitizenshipCountryId = testCountry.Id,
            MaritalStatus = "Single",
            HasRelativesInUS = true,
            HasJobOffer = false,
            EducationLevel = "Master's",
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false,
            ImmigrationGoal = "Work"
        };

        var user = new User
        {
            UserName = registrationDto.Email,
            Email = registrationDto.Email,
            FirstName = registrationDto.FirstName,
            MiddleName = registrationDto.MiddleName,
            LastName = registrationDto.LastName,
            PhoneNumber = registrationDto.PhoneNumber,
            Address1 = registrationDto.Address1,
            Address2 = registrationDto.Address2,
            City = registrationDto.City,
            State = registrationDto.State,
            Country = registrationDto.Country,
            PostalCode = registrationDto.PostalCode,
            DateOfBirth = registrationDto.DateOfBirth,
            CitizenshipCountryId = registrationDto.CitizenshipCountryId,
            MaritalStatus = registrationDto.MaritalStatus,
            PreferredLanguage = "en",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true
        };

        // Act
        var result = await _userManager.CreateAsync(user, registrationDto.Password);

        // Create UserProfile
        var userProfile = new UserProfile
        {
            UserId = user.Id,
            DateOfBirth = registrationDto.DateOfBirth,
            CitizenshipCountryId = registrationDto.CitizenshipCountryId,
            MaritalStatus = registrationDto.MaritalStatus,
            HasRelativesInUS = registrationDto.HasRelativesInUS,
            HasJobOffer = registrationDto.HasJobOffer,
            EducationLevel = registrationDto.EducationLevel,
            FearOfPersecution = registrationDto.FearOfPersecution,
            HasPastVisaDenials = registrationDto.HasPastVisaDenials,
            HasStatusViolations = registrationDto.HasStatusViolations,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // Assert
        Assert.That(result.Succeeded, Is.True, $"Full registration should succeed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        
        var createdUser = await _userManager.FindByEmailAsync(registrationDto.Email);
        Assert.That(createdUser, Is.Not.Null, "User should be created");
        Assert.That(createdUser.FirstName, Is.EqualTo("Complete"), "First name should match");
        Assert.That(createdUser.MiddleName, Is.EqualTo("Middle"), "Middle name should match");
        Assert.That(createdUser.LastName, Is.EqualTo("Test"), "Last name should match");
        Assert.That(createdUser.Address1, Is.EqualTo("123 Complete St"), "Address1 should match");
        Assert.That(createdUser.Address2, Is.EqualTo("Apt 1"), "Address2 should match");
        Assert.That(createdUser.CitizenshipCountryId, Is.EqualTo(testCountry.Id), "Citizenship country should match");

        var createdProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        Assert.That(createdProfile, Is.Not.Null, "User profile should be created");
        Assert.That(createdProfile.HasRelativesInUS, Is.True, "Has relatives flag should match");
        Assert.That(createdProfile.EducationLevel, Is.EqualTo("Master's"), "Education level should match");
    }

    [Test]
    public async Task CreateUserFromUI_StandardTestPassword_WorksForAllCategories()
    {
        // Arrange
        var categories = new[] { "Visit", "Work", "Study", "Family", "Investment", "Asylum", "Immigrate" };
        var standardPassword = "SecureTest123!"; // Standard test password for UI tests
        var createdUsers = new List<User>();

        // Act & Assert
        foreach (var category in categories)
        {
            var randomId = Guid.NewGuid().ToString("N")[..8];
            var testUser = new User
            {
                Email = $"{category.ToLower()}{randomId}@testing.com",
                UserName = $"{category.ToLower()}{randomId}@testing.com",
                FirstName = $"{category}Test",
                LastName = "User",
                Category = category,
                PhoneNumber = "555-0123",
                Address1 = "123 Test St",
                City = "Test City",
                State = "CA",
                Country = "United States",
                PostalCode = "12345",
                DateOfBirth = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(testUser, standardPassword);
            
            Assert.That(result.Succeeded, Is.True, $"User creation should succeed for category {category} with standard password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            
            // Verify password works
            var passwordCheck = await _userManager.CheckPasswordAsync(testUser, standardPassword);
            Assert.That(passwordCheck, Is.True, $"Standard password should work for {category} category user");
            
            createdUsers.Add(testUser);
        }

        Assert.That(createdUsers.Count, Is.EqualTo(categories.Length), "Should create users for all categories with standard password");
        
        // Verify all users can be retrieved and have correct categories
        foreach (var user in createdUsers)
        {
            var retrievedUser = await _userManager.FindByEmailAsync(user.Email!);
            Assert.That(retrievedUser, Is.Not.Null, $"User should be retrievable: {user.Email}");
            Assert.That(retrievedUser.Category, Is.Not.Null.And.Not.Empty, $"Category should be set for user: {user.Email}");
        }
    }
}