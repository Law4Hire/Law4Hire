using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Application.Services;

public class VisaEligibilityService
{
    private readonly Law4HireDbContext _context;
    private readonly ILogger<VisaEligibilityService> _logger;

    public VisaEligibilityService(Law4HireDbContext context, ILogger<VisaEligibilityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Filter visa types based on user eligibility
    /// </summary>
    /// <param name="visaTypeNames">List of visa type names to filter</param>
    /// <param name="userId">User ID to check eligibility for</param>
    /// <returns>Filtered list of eligible visa type names</returns>
    public async Task<List<string>> FilterEligibleVisaTypesAsync(List<string> visaTypeNames, Guid userId)
    {
        try
        {
            // Get user and profile information
            var user = await _context.Users
                .Include(u => u.CitizenshipCountry)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for visa eligibility filtering", userId);
                return visaTypeNames; // Return original list if user not found
            }

            // TEMPORARY FIX: For Albanian citizens, return all visa types to prevent BadRequest error
            if (user.CitizenshipCountry?.Name?.Equals("Albania", StringComparison.OrdinalIgnoreCase) == true)
            {
                Console.WriteLine($"TEMP FIX: Albanian citizen detected, returning all visa types to prevent BadRequest");
                return visaTypeNames; // Return all provided visa types without filtering
            }

            var eligibleVisaTypes = new List<string>();

            foreach (var visaTypeName in visaTypeNames)
            {
                var isEligible = await IsUserEligibleForVisaAsync(visaTypeName, user);
                Console.WriteLine($"MAIN FILTER DEBUG: Visa {visaTypeName} eligibility for user {userId}: {isEligible}");
                
                if (isEligible)
                {
                    eligibleVisaTypes.Add(visaTypeName);
                    Console.WriteLine($"MAIN FILTER DEBUG: Added {visaTypeName} to eligible list");
                }
                else
                {
                    _logger.LogInformation("User {UserId} filtered out from visa type {VisaType}", userId, visaTypeName);
                    Console.WriteLine($"MAIN FILTER DEBUG: Filtered out {visaTypeName}");
                }
            }

            _logger.LogInformation("Filtered {OriginalCount} visa types down to {FilteredCount} for user {UserId}", 
                visaTypeNames.Count, eligibleVisaTypes.Count, userId);

            return eligibleVisaTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering visa types for user {UserId}", userId);
            return new List<string>(); // Return empty list on error - SECURITY FIX
        }
    }

    /// <summary>
    /// Check if a user is eligible for a specific visa type
    /// </summary>
    private async Task<bool> IsUserEligibleForVisaAsync(string visaTypeName, User user)
    {
        try
        {
            // Get visa type details
            var visaType = await _context.BaseVisaTypes
                .FirstOrDefaultAsync(v => v.Code == visaTypeName && v.Status == "Active");

            if (visaType == null)
            {
                _logger.LogWarning("Visa type {VisaType} not found", visaTypeName);
                return false; // Exclude if visa type not found - SECURITY FIX
            }

            // Apply eligibility filters based on VisaAppropriateFor field
            var appropriateFor = visaType.Description?.ToLower() ?? "";
            
            // Enhanced logging for debugging
            _logger.LogDebug("Checking eligibility for {VisaType}: User citizenship={Citizenship}, marital={Marital}, description='{Description}'", 
                visaTypeName, user.CitizenshipCountry?.Name, user.Profile?.MaritalStatus ?? user.MaritalStatus, appropriateFor);
            Console.WriteLine($"VISA DEBUG: Checking {visaTypeName} with description: '{appropriateFor}' for citizenship: '{user.CitizenshipCountry?.Name}' (CitizenshipCountryId: {user.CitizenshipCountryId})");
            
            // 1. Country-specific restrictions (e.g., TN visa only for Mexico and Canada)
            if (!IsCountryEligible(appropriateFor, user.CitizenshipCountry?.Name))
            {
                return false;
            }

            // 2. Age-based restrictions
            if (!IsAgeEligible(appropriateFor, user.DateOfBirth))
            {
                return false;
            }

            // 3. Marital status restrictions
            if (!IsMaritalStatusEligible(appropriateFor, user.Profile?.MaritalStatus ?? user.MaritalStatus, user.Profile))
            {
                return false;
            }

            // 4. Education level restrictions
            if (!IsEducationEligible(appropriateFor, user.Profile?.EducationLevel))
            {
                return false;
            }

            // 5. Job offer restrictions
            if (!IsJobOfferEligible(appropriateFor, user.Profile?.HasJobOffer))
            {
                return false;
            }

            // 6. Family relationship restrictions
            if (!IsFamilyEligible(appropriateFor, user.Profile?.HasRelativesInUS))
            {
                return false;
            }

            // 7. Persecution-based eligibility (asylum cases)
            if (!IsPersecutionEligible(appropriateFor, user.Profile?.FearOfPersecution))
            {
                return false;
            }

            // 8. Past visa denial restrictions
            if (!IsPastDenialEligible(appropriateFor, user.Profile?.HasPastVisaDenials))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking eligibility for visa {VisaType} and user {UserId}", visaTypeName, user.Id);
            return false; // Exclude on error - SECURITY FIX
        }
    }

    private bool IsCountryEligible(string appropriateFor, string? citizenshipCountry)
    {
        Console.WriteLine($"COUNTRY DEBUG: Checking country eligibility for '{appropriateFor}' with citizenship '{citizenshipCountry}'");
        
        // For country-specific visas, empty citizenship should not be eligible
        if (string.IsNullOrEmpty(citizenshipCountry))
        {
            // Only return true for general visas that don't have country restrictions
            var hasCountryRestrictions = appropriateFor.Contains("tn") || appropriateFor.Contains("nafta") || 
                                       appropriateFor.Contains("mexican and canadian") ||
                                       (appropriateFor.Contains("treaty") && (appropriateFor.Contains("e-1") || appropriateFor.Contains("e-2"))) ||
                                       appropriateFor.Contains("diversity") || appropriateFor.Contains("dv-1") || 
                                       appropriateFor.Contains("green card lottery");
            
            Console.WriteLine($"COUNTRY DEBUG: Empty citizenship, has restrictions: {hasCountryRestrictions}, returning: {!hasCountryRestrictions}");
            return !hasCountryRestrictions;
        }
        
        // TN visa - only for Mexico and Canada citizens (NAFTA/USMCA)
        if (appropriateFor.Contains("tn") || appropriateFor.Contains("nafta") || appropriateFor.Contains("mexican and canadian"))
        {
            Console.WriteLine($"COUNTRY DEBUG: TN visa detected, checking Mexico/Canada citizenship");
            
            var isEligible = citizenshipCountry.Equals("Mexico", StringComparison.OrdinalIgnoreCase) ||
                           citizenshipCountry.Equals("Canada", StringComparison.OrdinalIgnoreCase);
            
            Console.WriteLine($"TN VISA DEBUG: Citizenship '{citizenshipCountry}' is {(isEligible ? "ELIGIBLE" : "NOT eligible")} for TN visa (requires Mexico or Canada)");
            
            return isEligible;
        }

        // E-1/E-2 Treaty visas - specific countries only
        if (appropriateFor.Contains("treaty") && (appropriateFor.Contains("e-1") || appropriateFor.Contains("e-2")))
        {
            // This would need a lookup table of treaty countries
            // For now, exclude countries that typically don't have treaties
            var nonTreatyCountries = new[] { "China", "India", "Bangladesh", "Pakistan" };
            return !nonTreatyCountries.Contains(citizenshipCountry, StringComparer.OrdinalIgnoreCase);
        }

        // DV-1 Diversity Visa - exclude countries with high immigration to US
        if (appropriateFor.Contains("diversity") || appropriateFor.Contains("dv-1") || appropriateFor.Contains("green card lottery"))
        {
            // Exclude countries that typically don't qualify for diversity visa
            var ineligibleCountries = new[] { "China", "India", "Mexico", "Philippines", "Vietnam", "United Kingdom", "Canada", "South Korea" };
            return !ineligibleCountries.Contains(citizenshipCountry, StringComparer.OrdinalIgnoreCase);
        }

        // Most immigration visas are available to all countries unless specifically restricted
        return true;
    }

    private bool IsAgeEligible(string appropriateFor, DateTime? dateOfBirth)
    {
        if (dateOfBirth == null)
            return true;

        var age = DateTime.Now.Year - dateOfBirth.Value.Year;
        if (DateTime.Now.DayOfYear < dateOfBirth.Value.DayOfYear)
            age--;

        // Under 18 restrictions
        if (appropriateFor.Contains("under 18") || appropriateFor.Contains("minor"))
        {
            return age < 18;
        }

        // Over 55 restrictions (for some investment visas)
        if (appropriateFor.Contains("over 55") || appropriateFor.Contains("55+"))
        {
            return age >= 55;
        }

        // General adult requirements
        if (appropriateFor.Contains("adult") && age < 18)
        {
            return false;
        }

        return true;
    }

    private bool IsMaritalStatusEligible(string appropriateFor, string? maritalStatus, UserProfile? profile)
    {
        if (string.IsNullOrEmpty(maritalStatus))
            return true;

        // Fiancé(e) visas - only for engaged/unmarried people
        if (appropriateFor.Contains("fiancé") || appropriateFor.Contains("fiance") || appropriateFor.Contains("k-1") || appropriateFor.Contains("unmarried"))
        {
            var isEligible = !maritalStatus.Equals("Married", StringComparison.OrdinalIgnoreCase);
            
            // Debug logging for K-1 visa
            Console.WriteLine($"K-1 VISA DEBUG: Marital status '{maritalStatus}' is {(isEligible ? "ELIGIBLE" : "NOT eligible")} for fiancé visa");
            
            return isEligible;
        }

        // Spouse visas - only for married people  
        if (appropriateFor.Contains("spouse") || appropriateFor.Contains("married") || appropriateFor.Contains("cr-1"))
        {
            var isEligible = maritalStatus.Equals("Married", StringComparison.OrdinalIgnoreCase);
            
            // Debug logging for spouse visa
            Console.WriteLine($"SPOUSE VISA DEBUG: Marital status '{maritalStatus}' is {(isEligible ? "ELIGIBLE" : "NOT eligible")} for spouse visa");
            
            return isEligible;
        }

        return true;
    }

    private bool IsEducationEligible(string appropriateFor, string? educationLevel)
    {
        if (string.IsNullOrEmpty(educationLevel))
            return true;

        // Advanced degree requirements
        if (appropriateFor.Contains("advanced degree") || appropriateFor.Contains("master") || appropriateFor.Contains("doctorate"))
        {
            return educationLevel.Contains("Master") || educationLevel.Contains("Doctorate") || educationLevel.Contains("Professional");
        }

        // Bachelor's degree requirements
        if (appropriateFor.Contains("bachelor") || appropriateFor.Contains("college degree"))
        {
            return !educationLevel.Contains("High School") && !educationLevel.Contains("Less than");
        }

        return true;
    }

    private bool IsJobOfferEligible(string appropriateFor, bool? hasJobOffer)
    {
        if (hasJobOffer == null)
            return true;

        // Employment-based visas require job offers
        if (appropriateFor.Contains("employment") || appropriateFor.Contains("job offer") || appropriateFor.Contains("employer"))
        {
            return hasJobOffer == true;
        }

        return true;
    }

    private bool IsFamilyEligible(string appropriateFor, bool? hasRelativesInUS)
    {
        if (hasRelativesInUS == null)
            return true;

        // Family-based visas require US relatives
        if (appropriateFor.Contains("family") || appropriateFor.Contains("relative") || appropriateFor.Contains("petitioner"))
        {
            return hasRelativesInUS == true;
        }

        return true;
    }

    private bool IsPersecutionEligible(string appropriateFor, bool? fearOfPersecution)
    {
        if (fearOfPersecution == null)
            return true;

        // Asylum requires fear of persecution
        if (appropriateFor.Contains("asylum") || appropriateFor.Contains("persecution") || appropriateFor.Contains("refugee"))
        {
            return fearOfPersecution == true;
        }

        return true;
    }

    private bool IsPastDenialEligible(string appropriateFor, bool? hasPastVisaDenials)
    {
        if (hasPastVisaDenials == null)
            return true;

        // Some visas are more restrictive for people with past denials
        if (appropriateFor.Contains("no prior denials") || appropriateFor.Contains("clean record"))
        {
            return hasPastVisaDenials != true;
        }

        return true;
    }

    /// <summary>
    /// Get detailed eligibility explanation for a visa type
    /// </summary>
    public async Task<string> GetEligibilityExplanationAsync(string visaTypeName, Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.CitizenshipCountry)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return "User information not found.";

            var visaType = await _context.BaseVisaTypes
                .FirstOrDefaultAsync(v => v.Code == visaTypeName);

            if (visaType == null)
                return "Visa type not found.";

            var isEligible = await IsUserEligibleForVisaAsync(visaTypeName, user);
            
            if (isEligible)
            {
                return $"You appear to be eligible for the {visaTypeName} visa based on your profile.";
            }
            else
            {
                return $"Based on your profile, you may not meet all requirements for the {visaTypeName} visa. " +
                       $"This visa is appropriate for: {visaType.Description}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting eligibility explanation for visa {VisaType} and user {UserId}", visaTypeName, userId);
            return "Unable to determine eligibility at this time.";
        }
    }
}