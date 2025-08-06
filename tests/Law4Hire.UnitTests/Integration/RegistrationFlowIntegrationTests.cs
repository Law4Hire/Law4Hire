using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Microsoft.Extensions.Logging;
using Moq;

namespace Law4Hire.UnitTests.Integration;

[TestFixture]
public class RegistrationFlowIntegrationTests
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
    public void RegistrationFlow_CompleteUserProfile_DoesNotThrowException()
    {
        // Arrange - Simulate a complete user registration
        var userRegistration = new UserRegistrationDto
        {
            FirstName = "John",
            LastName = "Doe", 
            MiddleName = "Middle",
            Email = "john.doe@testing.com",
            PhoneNumber = "+1234567890",
            Address1 = "123 Main St",
            Address2 = "Apt 1",
            City = "New York",
            State = "New York",
            Country = "United States",
            PostalCode = "10001",
            DateOfBirth = new DateTime(1990, 5, 15),
            CitizenshipCountryId = Guid.NewGuid(),
            MaritalStatus = "Single",
            HasRelativesInUS = false,
            HasJobOffer = true,
            EducationLevel = "Bachelor's Degree",
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false,
            ImmigrationGoal = "Work"
        };

        // Act & Assert - This should not throw any exceptions
        Assert.DoesNotThrow(() =>
        {
            var eligibleCategories = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, "Work");
            var ineligibleVisas = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);
            var summary = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, "Work");
            
            // Verify results are reasonable
            Assert.That(eligibleCategories, Is.Not.Null);
            Assert.That(eligibleCategories.Count, Is.GreaterThan(0));
            Assert.That(ineligibleVisas, Is.Not.Null);
            Assert.That(summary, Is.Not.Null);
            Assert.That(summary.Length, Is.GreaterThan(10), "Summary should be meaningful");
        });
    }

    [Test]
    public void RegistrationFlow_MinimalUserProfile_DoesNotThrowException()
    {
        // Arrange - Simulate minimal required fields
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = new DateTime(1985, 1, 1),
            MaritalStatus = null, // Could be null
            HasRelativesInUS = null,
            HasJobOffer = null,
            EducationLevel = null,
            FearOfPersecution = null,
            HasPastVisaDenials = null,
            HasStatusViolations = null,
            ImmigrationGoal = "Visit"
        };

        // Act & Assert - This should handle nulls gracefully
        Assert.DoesNotThrow(() =>
        {
            var eligibleCategories = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, "Visit");
            var ineligibleVisas = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);
            var summary = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, "Visit");
            
            // Verify results are reasonable
            Assert.That(eligibleCategories, Is.Not.Null);
            Assert.That(eligibleCategories.Count, Is.GreaterThan(0));
            Assert.That(ineligibleVisas, Is.Not.Null);
            Assert.That(summary, Is.Not.Null);
        });
    }

    [Test]
    public void RegistrationFlow_AsylumSeeker_DoesNotThrowException()
    {
        // Arrange - Simulate asylum seeker profile
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = new DateTime(1988, 10, 20),
            MaritalStatus = "Married",
            HasRelativesInUS = true,
            HasJobOffer = false,
            EducationLevel = "High School",
            FearOfPersecution = true, // Key asylum factor
            HasPastVisaDenials = true, // Complication
            HasStatusViolations = false,
            ImmigrationGoal = "Asylum"
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var eligibleCategories = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, "Asylum");
            var ineligibleVisas = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);
            var summary = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, "Asylum");
            
            // Verify asylum is included
            Assert.That(eligibleCategories, Contains.Item("Asylum"));
            Assert.That(summary, Does.Contain("asylum"));
            Assert.That(summary, Does.Contain("Previous denials"));
        });
    }

    [Test]
    public void RegistrationFlow_UnderageApplicant_DoesNotThrowException()
    {
        // Arrange - Test edge case of underage applicant
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = DateTime.Now.AddYears(-16), // 16 years old
            MaritalStatus = "Single",
            HasRelativesInUS = true,
            HasJobOffer = false,
            EducationLevel = "High School",
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false,
            ImmigrationGoal = "Family"
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var eligibleCategories = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, "Family");
            var ineligibleVisas = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);
            var summary = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, "Family");
            
            // Verify work visas are excluded for minors
            Assert.That(ineligibleVisas, Contains.Item("H-1B"));
            Assert.That(ineligibleVisas, Contains.Item("L-1A"));
            Assert.That(summary, Does.Contain("Age: 16"));
        });
    }

    [Test]
    public void RegistrationFlow_InvalidDateOfBirth_DoesNotThrowException()
    {
        // Arrange - Test with default DateTime (potential edge case)
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = default(DateTime), // This could cause issues
            MaritalStatus = "Single",
            ImmigrationGoal = "Visit"
        };

        // Act & Assert - Should handle gracefully
        Assert.DoesNotThrow(() =>
        {
            var eligibleCategories = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, "Visit");
            var ineligibleVisas = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);
            var summary = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, "Visit");
        });
    }

    [Test]
    public void RegistrationFlow_AllImmigrationGoals_DoNotThrowExceptions()
    {
        // Arrange - Test all possible immigration goals
        var userRegistration = new UserRegistrationDto
        {
            DateOfBirth = new DateTime(1985, 3, 10),
            MaritalStatus = "Single",
            HasRelativesInUS = false,
            HasJobOffer = false,
            FearOfPersecution = false,
            HasPastVisaDenials = false,
            HasStatusViolations = false
        };

        var immigrationGoals = new[] { "Visit", "Immigrate", "Investment", "Work", "Asylum", "Study", "Family" };

        // Act & Assert - Test each goal
        foreach (var goal in immigrationGoals)
        {
            Assert.DoesNotThrow(() =>
            {
                var eligibleCategories = _visaNarrowingService.GetEligibleVisaCategories(userRegistration, goal);
                var ineligibleVisas = _visaNarrowingService.GetIneligibleVisaTypes(userRegistration);
                var summary = _visaNarrowingService.GetVisaRecommendationSummary(userRegistration, goal);
                
                Assert.That(eligibleCategories, Is.Not.Null, $"Failed for goal: {goal}");
                Assert.That(ineligibleVisas, Is.Not.Null, $"Failed for goal: {goal}");
                Assert.That(summary, Is.Not.Null, $"Failed for goal: {goal}");
            }, $"Exception thrown for immigration goal: {goal}");
        }
    }
}