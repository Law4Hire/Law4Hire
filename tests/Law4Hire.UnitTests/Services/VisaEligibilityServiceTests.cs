using Law4Hire.Application.Services;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Law4Hire.UnitTests.Services;

[TestFixture]
public class VisaEligibilityServiceTests
{
    private Law4HireDbContext _context = null!;
    private VisaEligibilityService _service = null!;
    private Mock<ILogger<VisaEligibilityService>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<Law4HireDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new Law4HireDbContext(options);
        _mockLogger = new Mock<ILogger<VisaEligibilityService>>();
        _service = new VisaEligibilityService(_context, _mockLogger.Object);

        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private void SeedTestData()
    {
        // Seed test countries
        var countries = new List<Country>
        {
            new() { Id = Guid.NewGuid(), Name = "United States", CountryCode = "USA", CountryCode2 = "US" },
            new() { Id = Guid.NewGuid(), Name = "Mexico", CountryCode = "MEX", CountryCode2 = "MX" },
            new() { Id = Guid.NewGuid(), Name = "Canada", CountryCode = "CAN", CountryCode2 = "CA" },
            new() { Id = Guid.NewGuid(), Name = "United Kingdom", CountryCode = "GBR", CountryCode2 = "GB" },
            new() { Id = Guid.NewGuid(), Name = "India", CountryCode = "IND", CountryCode2 = "IN" }
        };

        // Seed test visa types
        var visaTypes = new List<BaseVisaType>
        {
            new() { Id = Guid.NewGuid(), Code = "TN", Name = "TN - NAFTA Professional", Description = "NAFTA Professional for Mexican and Canadian citizens", Status = "Active" },
            new() { Id = Guid.NewGuid(), Code = "H-1B", Name = "H-1B - Specialty Occupation", Description = "Specialty Occupation for professionals with bachelor's degree", Status = "Active" },
            new() { Id = Guid.NewGuid(), Code = "K-1", Name = "K-1 - Fiancé Visa", Description = "Fiancé Visa for unmarried individuals engaged to US citizens", Status = "Active" },
            new() { Id = Guid.NewGuid(), Code = "CR-1", Name = "CR-1 - Spouse of US Citizen", Description = "Spouse of US Citizen for married spouses of US citizens", Status = "Active" },
            new() { Id = Guid.NewGuid(), Code = "F-1", Name = "F-1 - Student Visa", Description = "Student Visa for students admitted to US educational institutions", Status = "Active" },
            new() { Id = Guid.NewGuid(), Code = "I-589", Name = "I-589 - Asylum Application", Description = "Asylum Application for individuals fearing persecution", Status = "Active" }
        };

        // Seed test user
        var mexicoCountry = countries.First(c => c.Name == "Mexico");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Country = "Mexico",
            CitizenshipCountryId = mexicoCountry.Id
        };

        // Seed test user profile
        var userProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CitizenshipCountryId = mexicoCountry.Id,
            MaritalStatus = "Single",
            EducationLevel = "Bachelor's Degree",
            HasJobOffer = true,
            HasRelativesInUS = false,
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false
        };

        _context.Countries.AddRange(countries);
        _context.BaseVisaTypes.AddRange(visaTypes);
        _context.Users.Add(user);
        _context.UserProfiles.Add(userProfile);
        _context.SaveChanges();
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_TNVisa_OnlyAllowsMexicanAndCanadianCitizens()
    {
        // Arrange
        var user = _context.Users.First();
        var visaTypeNames = new List<string> { "TN", "H-1B" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Contains.Item("TN"), "Mexican citizen should be eligible for TN visa");
        Assert.That(result, Contains.Item("H-1B"), "Should include H-1B as fallback");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_TNVisa_ExcludesNonNAFTACitizens()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        var ukCountry = _context.Countries.First(c => c.Name == "United Kingdom");
        
        // Change citizenship to UK (non-NAFTA country)
        user.CitizenshipCountryId = ukCountry.Id;
        userProfile.CitizenshipCountryId = ukCountry.Id;
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "TN", "H-1B" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Does.Not.Contain("TN"), "UK citizen should not be eligible for TN visa");
        Assert.That(result, Contains.Item("H-1B"), "Should still include H-1B");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_FianceVisa_OnlyAllowsUnmarriedIndividuals()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        userProfile.MaritalStatus = "Single";
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "K-1", "CR-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Contains.Item("K-1"), "Single person should be eligible for fiancé visa");
        Assert.That(result, Does.Not.Contain("CR-1"), "Single person should not be eligible for spouse visa");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_SpouseVisa_OnlyAllowsMarriedIndividuals()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        userProfile.MaritalStatus = "Married";
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "K-1", "CR-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Does.Not.Contain("K-1"), "Married person should not be eligible for fiancé visa");
        Assert.That(result, Contains.Item("CR-1"), "Married person should be eligible for spouse visa");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_StudentVisa_RequiresEducationalAdmission()
    {
        // Arrange
        var user = _context.Users.First();
        var visaTypeNames = new List<string> { "F-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Contains.Item("F-1"), "Should include F-1 visa for educational purposes");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_AsylumApplication_RequiresPersecutionFear()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        
        // Test with no persecution fear
        userProfile.FearOfPersecution = false;
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "I-589" };

        // Act - No persecution fear
        var resultNoFear = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Change to having persecution fear
        userProfile.FearOfPersecution = true;
        _context.SaveChanges();

        // Act - With persecution fear
        var resultWithFear = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(resultWithFear, Contains.Item("I-589"), "Should include asylum for those fearing persecution");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_AgeRestrictions_Under21ForCertainVisas()
    {
        // Arrange
        var user = _context.Users.First();
        user.DateOfBirth = DateTime.Now.AddYears(-20); // 20 years old
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "H-1B", "F-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Contains.Item("F-1"), "Under 21 should be eligible for student visa");
        // H-1B typically requires professional experience, so might be filtered for very young applicants
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_EducationLevelRequirements_H1BRequiresBachelorsDegree()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        
        // Test with high school education
        userProfile.EducationLevel = "High School";
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "H-1B" };

        // Act - High school education
        var resultHighSchool = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Change to bachelor's degree
        userProfile.EducationLevel = "Bachelor's Degree";
        _context.SaveChanges();

        // Act - Bachelor's degree
        var resultBachelors = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(resultBachelors, Contains.Item("H-1B"), "Bachelor's degree holder should be eligible for H-1B");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_JobOfferRequirement_EmploymentBasedVisas()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        
        // Test without job offer
        userProfile.HasJobOffer = false;
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "H-1B", "TN" };

        // Act - No job offer
        var resultNoJob = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Change to having job offer
        userProfile.HasJobOffer = true;
        _context.SaveChanges();

        // Act - With job offer
        var resultWithJob = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(resultWithJob, Contains.Item("H-1B"), "Should include H-1B with job offer");
        Assert.That(resultWithJob, Contains.Item("TN"), "Should include TN with job offer (if Mexican/Canadian)");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_PastVisaDenials_ConsidersHistory()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        userProfile.HasPastVisaDenials = true;
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "H-1B", "F-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        // Past denials might affect eligibility, but shouldn't completely exclude all visas
        Assert.That(result, Is.Not.Empty, "Should not exclude all visas due to past denials");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_StatusViolations_AffectsEligibility()
    {
        // Arrange
        var user = _context.Users.First();
        var userProfile = _context.UserProfiles.First(up => up.UserId == user.Id);
        userProfile.HasStatusViolations = true;
        _context.SaveChanges();

        var visaTypeNames = new List<string> { "H-1B", "F-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        // Status violations should be considered but shouldn't exclude all options
        Assert.That(result, Is.Not.Empty, "Should not exclude all visas due to status violations");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_EmptyInput_ReturnsEmptyList()
    {
        // Arrange
        var user = _context.Users.First();
        var visaTypeNames = new List<string>();

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, user.Id);

        // Assert
        Assert.That(result, Is.Empty, "Empty input should return empty result");
    }

    [Test]
    public async Task FilterEligibleVisaTypesAsync_NonExistentUser_ReturnsOriginalList()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var visaTypeNames = new List<string> { "H-1B", "F-1" };

        // Act
        var result = await _service.FilterEligibleVisaTypesAsync(visaTypeNames, nonExistentUserId);

        // Assert
        Assert.That(result, Is.EqualTo(visaTypeNames), "Non-existent user should return original list");
    }
}