using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Application.Services;

public class VisaNarrowingService
{
    private readonly ILogger<VisaNarrowingService> _logger;

    public VisaNarrowingService(ILogger<VisaNarrowingService> logger)
    {
        _logger = logger;
    }

    public List<string> GetEligibleVisaCategories(UserRegistrationDto userRegistration, string immigrationGoal)
    {
        var eligibleCategories = new List<string>();
        
        // Start with the primary goal category
        var primaryCategory = immigrationGoal switch
        {
            "Visit" => "Visit",
            "Immigrate" => "Immigrate", 
            "Investment" => "Investment",
            "Work" => "Work",
            "Asylum" => "Asylum",
            "Study" => "Study",
            "Family" => "Family",
            _ => "Visit"
        };
        
        eligibleCategories.Add(primaryCategory);

        // Apply narrowing logic based on user responses
        eligibleCategories = ApplyAgeBasedFilters(eligibleCategories, userRegistration);
        eligibleCategories = ApplyMaritalStatusFilters(eligibleCategories, userRegistration);
        eligibleCategories = ApplyAsylumFilters(eligibleCategories, userRegistration);
        eligibleCategories = ApplyFamilyFilters(eligibleCategories, userRegistration);
        eligibleCategories = ApplyWorkFilters(eligibleCategories, userRegistration);
        eligibleCategories = ApplyDenialFilters(eligibleCategories, userRegistration);

        return eligibleCategories.Distinct().ToList();
    }

    public List<string> GetIneligibleVisaTypes(UserRegistrationDto userRegistration)
    {
        var ineligibleVisas = new List<string>();
        
        // Age-based exclusions
        var age = GetAge(userRegistration.DateOfBirth);
        if (age > 21)
        {
            // Cannot apply for child-specific visas
            ineligibleVisas.AddRange(new[] { "IR-2", "CR-2", "F2B", "K-2", "K-4" });
        }
        
        if (age < 18)
        {
            // Minors typically cannot apply for work visas independently
            ineligibleVisas.AddRange(new[] { "H-1B", "L-1A", "L-1B", "O-1", "E-1", "E-2", "EB-1", "EB-2", "EB-3" });
        }

        // Marital status exclusions
        if (userRegistration.MaritalStatus?.Equals("Married", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Married individuals cannot apply for fiancé visas
            ineligibleVisas.AddRange(new[] { "K-1", "K-2" });
        }
        else if (userRegistration.MaritalStatus?.Equals("Single", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Single individuals cannot apply for spouse visas
            ineligibleVisas.AddRange(new[] { "IR-1", "CR-1", "K-3" });
        }

        // Job offer requirements
        if (userRegistration.HasJobOffer != true)
        {
            // Many work visas require job offers
            ineligibleVisas.AddRange(new[] { "H-1B", "H-2A", "H-2B", "L-1A", "L-1B", "O-1", "P-1", "P-2", "P-3", "TN" });
        }

        // Persecution-based logic
        if (userRegistration.FearOfPersecution != true)
        {
            // Not eligible for asylum without persecution fear
            ineligibleVisas.AddRange(new[] { "I-589" });
        }

        // Relatives in US
        if (userRegistration.HasRelativesInUS != true)
        {
            // Family reunification visas require relatives
            ineligibleVisas.AddRange(new[] { "IR-1", "IR-2", "IR-5", "CR-1", "CR-2", "F1", "F2A", "F2B", "F3", "F4" });
        }

        // Previous denials impact
        if (userRegistration.HasPastVisaDenials == true)
        {
            _logger.LogInformation("User has previous visa denials - may face additional scrutiny");
            // Don't automatically exclude but note for additional review
        }

        // Status violations impact
        if (userRegistration.HasStatusViolations == true)
        {
            _logger.LogInformation("User has status violations - may face bars to admission");
            // May be subject to bars, needs legal review
        }

        return ineligibleVisas.Distinct().ToList();
    }

    private List<string> ApplyAgeBasedFilters(List<string> categories, UserRegistrationDto user)
    {
        var age = GetAge(user.DateOfBirth);
        
        if (age < 18)
        {
            // Minors typically need guardians for most processes
            categories.Remove("Work");
            categories.Remove("Investment");
        }
        
        return categories;
    }

    private List<string> ApplyMaritalStatusFilters(List<string> categories, UserRegistrationDto user)
    {
        if (user.MaritalStatus?.Equals("Married", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Married individuals have family reunification options
            if (!categories.Contains("Family"))
            {
                categories.Add("Family");
            }
        }
        
        return categories;
    }

    private List<string> ApplyAsylumFilters(List<string> categories, UserRegistrationDto user)
    {
        if (user.FearOfPersecution == true)
        {
            // Fear of persecution makes asylum a viable option
            if (!categories.Contains("Asylum"))
            {
                categories.Add("Asylum");
            }
        }
        else
        {
            // No persecution fear means asylum is not applicable
            categories.Remove("Asylum");
        }
        
        return categories;
    }

    private List<string> ApplyFamilyFilters(List<string> categories, UserRegistrationDto user)
    {
        if (user.HasRelativesInUS == true)
        {
            // Having relatives opens family reunification options
            if (!categories.Contains("Family"))
            {
                categories.Add("Family");
            }
        }
        else
        {
            // No relatives means family reunification is not applicable
            categories.Remove("Family");
        }
        
        return categories;
    }

    private List<string> ApplyWorkFilters(List<string> categories, UserRegistrationDto user)
    {
        if (user.HasJobOffer == true)
        {
            // Job offer opens work-based options
            if (!categories.Contains("Work"))
            {
                categories.Add("Work");
            }
        }
        
        return categories;
    }

    private List<string> ApplyDenialFilters(List<string> categories, UserRegistrationDto user)
    {
        if (user.HasPastVisaDenials == true || user.HasStatusViolations == true)
        {
            _logger.LogWarning("User has complications (denials/violations) that may affect eligibility");
            // Don't automatically remove categories but flag for review
        }
        
        return categories;
    }

    private int GetAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }
        
        return age;
    }

    public string GetVisaRecommendationSummary(UserRegistrationDto user, string immigrationGoal)
    {
        var age = GetAge(user.DateOfBirth);
        var eligibleCategories = GetEligibleVisaCategories(user, immigrationGoal);
        var ineligibleVisas = GetIneligibleVisaTypes(user);
        
        var summary = $"Based on your profile (Age: {age}, Marital Status: {user.MaritalStatus ?? "Not specified"}), ";
        
        if (user.FearOfPersecution == true)
        {
            summary += "asylum options are available due to persecution concerns. ";
        }
        
        if (user.HasRelativesInUS == true)
        {
            summary += "Family reunification visas may be applicable. ";
        }
        
        if (user.HasJobOffer == true)
        {
            summary += "Work-based visas are viable with your job offer. ";
        }
        
        if (user.HasPastVisaDenials == true)
        {
            summary += "Previous denials may require additional documentation and legal review. ";
        }
        
        if (user.HasStatusViolations == true)
        {
            summary += "Immigration violations may create bars to admission requiring legal counsel. ";
        }
        
        summary += $"Recommended categories: {string.Join(", ", eligibleCategories)}.";
        
        return summary;
    }
}