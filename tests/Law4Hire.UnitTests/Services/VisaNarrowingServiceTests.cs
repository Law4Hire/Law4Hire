using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Microsoft.Extensions.Logging;
using Moq;

namespace Law4Hire.UnitTests.Services;

[TestFixture]
public class VisaNarrowingServiceTests
{
    private VisaNarrowingService _visaNarrowingService;
    private Mock<ILogger<VisaNarrowingService>> _mockLogger;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<VisaNarrowingService>>();
        _visaNarrowingService = new VisaNarrowingService(_mockLogger.Object);
    }

    [Test]
    public void GetEligibleVisaCategories_VisitGoal_ReturnsVisitCategory()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single"
        };
        var immigrationGoal = "Visit";

        // Act
        var result = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, immigrationGoal);

        // Assert
        Assert.That(result, Contains.Item("Visit"));
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public void GetEligibleVisaCategories_AsylumFear_AddsAsylumCategory()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single",
            FearOfPersecution = true
        };
        var immigrationGoal = "Visit";

        // Act
        var result = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, immigrationGoal);

        // Assert
        Assert.That(result, Contains.Item("Asylum"));
        Assert.That(result, Contains.Item("Visit"));
    }

    [Test]
    public void GetEligibleVisaCategories_HasRelativesInUS_AddsFamilyCategory()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Married",
            HasRelativesInUS = true
        };
        var immigrationGoal = "Visit";

        // Act
        var result = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, immigrationGoal);

        // Assert
        Assert.That(result, Contains.Item("Family"));
        Assert.That(result, Contains.Item("Visit"));
    }

    [Test]
    public void GetEligibleVisaCategories_HasJobOffer_AddsWorkCategory()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single",
            HasJobOffer = true
        };
        var immigrationGoal = "Visit";

        // Act
        var result = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, immigrationGoal);

        // Assert
        Assert.That(result, Contains.Item("Work"));
        Assert.That(result, Contains.Item("Visit"));
    }

    [Test]
    public void GetIneligibleVisaTypes_Over21_ExcludesChildVisas()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-25), // 25 years old
            MaritalStatus = "Single"
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("IR-2")); // Child visa
        Assert.That(result, Contains.Item("CR-2")); // Child visa
        Assert.That(result, Contains.Item("K-2"));  // Child visa
    }

    [Test]
    public void GetIneligibleVisaTypes_Under18_ExcludesWorkVisas()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-16), // 16 years old
            MaritalStatus = "Single"
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("H-1B"));
        Assert.That(result, Contains.Item("L-1A"));
        Assert.That(result, Contains.Item("O-1"));
        Assert.That(result, Contains.Item("EB-1"));
    }

    [Test]
    public void GetIneligibleVisaTypes_Married_ExcludesFianceVisas()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Married"
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("K-1")); // Fiancé visa
        Assert.That(result, Contains.Item("K-2")); // Fiancé child visa
    }

    [Test]
    public void GetIneligibleVisaTypes_Single_ExcludesSpouseVisas()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single"
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("IR-1")); // Spouse visa
        Assert.That(result, Contains.Item("CR-1")); // Spouse visa
        Assert.That(result, Contains.Item("K-3"));  // Spouse visa
    }

    [Test]
    public void GetIneligibleVisaTypes_NoJobOffer_ExcludesWorkVisas()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single",
            HasJobOffer = false
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("H-1B"));
        Assert.That(result, Contains.Item("L-1A"));
        Assert.That(result, Contains.Item("O-1"));
        Assert.That(result, Contains.Item("TN"));
    }

    [Test]
    public void GetIneligibleVisaTypes_NoFearOfPersecution_ExcludesAsylum()
    {
        // Arrange  
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single",
            FearOfPersecution = false
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("I-589")); // Asylum application
    }

    [Test]
    public void GetIneligibleVisaTypes_NoRelativesInUS_ExcludesFamilyVisas()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-30),
            MaritalStatus = "Single",
            HasRelativesInUS = false
        };

        // Act
        var result = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);

        // Assert
        Assert.That(result, Contains.Item("IR-1"));
        Assert.That(result, Contains.Item("IR-2"));
        Assert.That(result, Contains.Item("F1"));
        Assert.That(result, Contains.Item("F3"));
    }

    [Test]
    public void GetVisaRecommendationSummary_ComprehensiveProfile_ReturnsDetailedSummary()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-35), // 35 years old
            MaritalStatus = "Married",
            HasRelativesInUS = true,
            HasJobOffer = true,
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false
        };
        var immigrationGoal = "Work";

        // Act
        var result = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, immigrationGoal);

        // Assert
        Assert.That(result, Does.Contain("Age: 35"));
        Assert.That(result, Does.Contain("Married"));
        Assert.That(result, Does.Contain("Family reunification visas may be applicable"));
        Assert.That(result, Does.Contain("Work-based visas are viable"));
        Assert.That(result, Does.Not.Contain("asylum"));
        Assert.That(result, Does.Not.Contain("Previous denials"));
        Assert.That(result, Does.Not.Contain("violations"));
    }

    [Test]
    public void GetVisaRecommendationSummary_WithComplications_IncludesWarnings()
    {
        // Arrange
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-28),
            MaritalStatus = "Single",
            HasPastVisaDenials = true,
            HasStatusViolations = true,
            FearOfPersecution = true
        };
        var immigrationGoal = "Asylum";

        // Act
        var result = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, immigrationGoal);

        // Assert
        Assert.That(result, Does.Contain("asylum options are available"));
        Assert.That(result, Does.Contain("Previous denials may require additional documentation"));
        Assert.That(result, Does.Contain("Immigration violations may create bars to admission"));
    }

    [Test]
    public void GetAge_ValidBirthDate_ReturnsCorrectAge()
    {
        // Arrange & Act - Using reflection to test private method
        var method = typeof(VisaNarrowingService).GetMethod("GetAge", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var birthDate = new DateTime(1990, 6, 15);
        var age = (int)method!.Invoke(_visaNarrowingService, new object[] { birthDate })!;

        // Assert
        var expectedAge = DateTime.Today.Year - 1990;
        if (DateTime.Today < new DateTime(DateTime.Today.Year, 6, 15))
            expectedAge--;
            
        Assert.That(age, Is.EqualTo(expectedAge));
    }
}