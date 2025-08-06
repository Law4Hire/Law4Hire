using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Application.Services;

/// <summary>
/// Enhanced visa interview service with precise disqualifying logic
/// Gets down to exactly 1 recommendation using purpose-category mapping and qualifying questions
/// </summary>
public class EnhancedVisaInterviewService
{
    private readonly Law4HireDbContext _context;
    private readonly ILogger<EnhancedVisaInterviewService> _logger;

    // Precise category-purpose mapping with subcategories
    private readonly Dictionary<string, CategoryPurposeMapping> _purposeMappings = new()
    {
        ["Tourism & Visit"] = new()
        {
            PrimaryPurpose = "Tourism & Visit",
            RequiredSubcategories = new[] { "TOURISM", "BUSINESS_VISIT", "MEDICAL_TREATMENT", "TRANSIT" },
            DisqualifyingAnswers = new Dictionary<string, string[]>
            {
                ["Are you planning to work in the U.S.?"] = new[] { "YES" },
                ["Do you intend to immigrate permanently?"] = new[] { "YES" },
                ["Are you enrolled in a U.S. school?"] = new[] { "YES" }
            }
        },
        ["Study & Exchange"] = new()
        {
            PrimaryPurpose = "Study & Exchange", 
            RequiredSubcategories = new[] { "ACADEMIC_STUDY", "VOCATIONAL_STUDY", "EXCHANGE_PROGRAM" },
            DisqualifyingAnswers = new Dictionary<string, string[]>
            {
                ["Do you have an approved Form I-20 from a SEVP-certified school?"] = new[] { "NO" },
                ["Are you primarily coming to work rather than study?"] = new[] { "YES" }
            }
        },
        ["Business & Employment"] = new()
        {
            PrimaryPurpose = "Business & Employment",
            RequiredSubcategories = new[] { "SPECIALTY_WORKER", "TEMPORARY_WORKER", "INTRACOMPANY_TRANSFER", "TREATY_BUSINESS", "EXTRAORDINARY_ABILITY" },
            DisqualifyingAnswers = new Dictionary<string, string[]>
            {
                ["Do you have a job offer or work authorization?"] = new[] { "NO" },
                ["Are you coming primarily for tourism?"] = new[] { "YES" }
            }
        },
        ["Immigrate"] = new()
        {
            PrimaryPurpose = "Immigrate",
            RequiredSubcategories = new[] { "FAMILY_BASED", "EMPLOYMENT_BASED", "DIVERSITY_LOTTERY", "SPECIAL_CIRCUMSTANCES" },
            SubcategoryMappings = new Dictionary<string, string[]>
            {
                ["FAMILY_BASED"] = new[] { "IR-", "CR-", "K-", "V-" },
                ["EMPLOYMENT_BASED"] = new[] { "EB-" },
                ["SPECIAL_CIRCUMSTANCES"] = new[] { "T-", "U-", "SIJS", "VAWA", "SIV" }
            },
            DisqualifyingAnswers = new Dictionary<string, string[]>
            {
                // If no family relationships, can't use family-based immigration
                ["Do you have a U.S. citizen or permanent resident spouse, parent, or child?"] = new[] { "NO" },
                ["Are you engaged to a U.S. citizen?"] = new[] { "NO" }
            }
        }
    };

    public EnhancedVisaInterviewService(Law4HireDbContext context, ILogger<EnhancedVisaInterviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Start interview with enhanced purpose classification
    /// </summary>
    public async Task<EnhancedInterviewResponse> StartInterviewAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Starting enhanced visa interview for user {UserId}", userId);

            var purposes = _purposeMappings.Keys.ToList();

            return new EnhancedInterviewResponse
            {
                QuestionType = "PURPOSE_CLASSIFICATION",
                Question = "What is your primary purpose for coming to the United States?",
                Options = purposes.Select(p => new EnhancedInterviewOption
                {
                    Value = p,
                    Label = GetPurposeDescription(p),
                    DisqualifyingQuestions = new List<string>()
                }).ToList(),
                NextStep = "PURPOSE_VALIDATION",
                SessionData = new Dictionary<string, object>
                {
                    ["userId"] = userId,
                    ["step"] = "PURPOSE_CLASSIFICATION"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting enhanced interview for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Process answer with enhanced disqualifying logic
    /// </summary>
    public async Task<EnhancedInterviewResponse> ProcessAnswerAsync(EnhancedInterviewRequest request)
    {
        try
        {
            _logger.LogInformation("Processing enhanced answer for user {UserId}, step {Step}", 
                request.UserId, request.CurrentStep);

            return request.CurrentStep switch
            {
                "PURPOSE_CLASSIFICATION" => await ProcessPurposeClassificationAsync(request),
                "PURPOSE_VALIDATION" => await ProcessPurposeValidationAsync(request),
                "SUBCATEGORY_SELECTION" => await ProcessSubcategorySelectionAsync(request),
                "QUALIFYING_QUESTIONS" => await ProcessQualifyingQuestionsAsync(request),
                "DISQUALIFYING_CHECK" => await ProcessDisqualifyingCheckAsync(request),
                _ => throw new ArgumentException($"Unknown step: {request.CurrentStep}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing enhanced answer for user {UserId}", request.UserId);
            throw;
        }
    }

    private async Task<EnhancedInterviewResponse> ProcessPurposeClassificationAsync(EnhancedInterviewRequest request)
    {
        var selectedPurpose = request.Answer;
        
        if (!_purposeMappings.ContainsKey(selectedPurpose))
        {
            throw new ArgumentException($"Invalid purpose: {selectedPurpose}");
        }

        var mapping = _purposeMappings[selectedPurpose];

        // Get initial disqualifying questions for this purpose
        var disqualifyingQuestions = GetDisqualifyingQuestionsForPurpose(selectedPurpose);
        
        if (disqualifyingQuestions.Any())
        {
            return new EnhancedInterviewResponse
            {
                QuestionType = "PURPOSE_VALIDATION",
                Question = disqualifyingQuestions.First(),
                Options = new List<EnhancedInterviewOption>
                {
                    new() { Value = "YES", Label = "Yes" },
                    new() { Value = "NO", Label = "No" }
                },
                NextStep = "SUBCATEGORY_SELECTION",
                SessionData = new Dictionary<string, object>
                {
                    ["userId"] = request.UserId,
                    ["selectedPurpose"] = selectedPurpose,
                    ["disqualifyingQuestions"] = disqualifyingQuestions,
                    ["currentDisqualifyingIndex"] = 0
                }
            };
        }

        // No disqualifying questions, go to subcategory
        return await ProcessSubcategorySelectionAsync(request);
    }

    private async Task<EnhancedInterviewResponse> ProcessPurposeValidationAsync(EnhancedInterviewRequest request)
    {
        var selectedPurpose = (string)request.SessionData["selectedPurpose"];
        var disqualifyingQuestions = (List<string>)request.SessionData["disqualifyingQuestions"];
        var currentIndex = (int)request.SessionData["currentDisqualifyingIndex"];
        var currentQuestion = disqualifyingQuestions[currentIndex];

        // Check if answer disqualifies this purpose
        var mapping = _purposeMappings[selectedPurpose];
        if (mapping.DisqualifyingAnswers.ContainsKey(currentQuestion))
        {
            var disqualifyingAnswers = mapping.DisqualifyingAnswers[currentQuestion];
            if (disqualifyingAnswers.Contains(request.Answer))
            {
                // Purpose disqualified - suggest alternative
                return new EnhancedInterviewResponse
                {
                    QuestionType = "PURPOSE_DISQUALIFIED",
                    Question = $"Based on your answer, {selectedPurpose} may not be the right purpose. Let me suggest alternatives.",
                    AlternativePurposes = await GetAlternativePurposesAsync(selectedPurpose, request.Answer),
                    NextStep = "PURPOSE_CLASSIFICATION"
                };
            }
        }

        // Check if more disqualifying questions remain
        if (currentIndex + 1 < disqualifyingQuestions.Count)
        {
            var nextQuestion = disqualifyingQuestions[currentIndex + 1];
            request.SessionData["currentDisqualifyingIndex"] = currentIndex + 1;
            
            return new EnhancedInterviewResponse
            {
                QuestionType = "PURPOSE_VALIDATION",
                Question = nextQuestion,
                Options = new List<EnhancedInterviewOption>
                {
                    new() { Value = "YES", Label = "Yes" },
                    new() { Value = "NO", Label = "No" }
                },
                NextStep = "SUBCATEGORY_SELECTION",
                SessionData = request.SessionData
            };
        }

        // All validation passed, move to subcategory
        return await ProcessSubcategorySelectionAsync(request);
    }

    private async Task<EnhancedInterviewResponse> ProcessSubcategorySelectionAsync(EnhancedInterviewRequest request)
    {
        var selectedPurpose = (string)request.SessionData["selectedPurpose"];
        var mapping = _purposeMappings[selectedPurpose];

        // Special handling for family-based immigration
        if (selectedPurpose == "Immigrate" && request.SessionData.ContainsKey("familyRelationship"))
        {
            var hasFamily = (bool)request.SessionData["familyRelationship"];
            if (!hasFamily)
            {
                // Remove family-based subcategory
                var subcategories = mapping.RequiredSubcategories.Where(s => s != "FAMILY_BASED").ToList();
                mapping = new CategoryPurposeMapping
                {
                    PrimaryPurpose = mapping.PrimaryPurpose,
                    RequiredSubcategories = subcategories.ToArray(),
                    SubcategoryMappings = mapping.SubcategoryMappings,
                    DisqualifyingAnswers = mapping.DisqualifyingAnswers
                };
            }
        }

        var options = new List<EnhancedInterviewOption>();
        foreach (var sub in mapping.RequiredSubcategories)
        {
            options.Add(new EnhancedInterviewOption
            {
                Value = sub,
                Label = GetSubcategoryDescription(sub),
                EstimatedVisaCount = await GetVisaCountForSubcategoryAsync(selectedPurpose, sub)
            });
        }

        return new EnhancedInterviewResponse
        {
            QuestionType = "SUBCATEGORY_SELECTION",
            Question = $"Which of these best describes your specific {selectedPurpose.ToLower()} situation?",
            Options = options,
            NextStep = "QUALIFYING_QUESTIONS",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = request.UserId,
                ["selectedPurpose"] = selectedPurpose,
                ["subcategoryMapping"] = mapping
            }
        };
    }

    private async Task<EnhancedInterviewResponse> ProcessQualifyingQuestionsAsync(EnhancedInterviewRequest request)
    {
        var selectedPurpose = (string)request.SessionData["selectedPurpose"];
        var selectedSubcategory = request.Answer;
        
        // Get visa codes for this purpose + subcategory combination
        var visaCodes = await GetVisaCodesForPurposeAndSubcategoryAsync(selectedPurpose, selectedSubcategory);

        if (visaCodes.Count == 0)
        {
            return new EnhancedInterviewResponse
            {
                QuestionType = "NO_MATCH",
                Question = "We couldn't find any matching visa types for your situation. Please consult with an immigration attorney.",
                NextStep = "COMPLETE"
            };
        }

        if (visaCodes.Count == 1)
        {
            // Perfect - exactly one match
            return await GenerateFinalRecommendationAsync(request.UserId, visaCodes[0]);
        }

        // Need to narrow down further with qualifying questions
        var discriminatingQuestion = await GetMostDiscriminatingQuestionAsync(visaCodes);
        
        return new EnhancedInterviewResponse
        {
            QuestionType = "QUALIFYING_QUESTIONS", 
            Question = discriminatingQuestion,
            Options = new List<EnhancedInterviewOption>
            {
                new() { Value = "YES", Label = "Yes" },
                new() { Value = "NO", Label = "No" }
            },
            NextStep = "DISQUALIFYING_CHECK",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = request.UserId,
                ["selectedPurpose"] = selectedPurpose,
                ["selectedSubcategory"] = selectedSubcategory,
                ["candidateVisaCodes"] = visaCodes,
                ["currentQuestion"] = discriminatingQuestion
            }
        };
    }

    private async Task<EnhancedInterviewResponse> ProcessDisqualifyingCheckAsync(EnhancedInterviewRequest request)
    {
        var candidateVisaCodes = (List<string>)request.SessionData["candidateVisaCodes"];
        var currentQuestion = (string)request.SessionData["currentQuestion"];
        var answer = request.Answer;

        // Filter visa codes based on the qualifying question answer
        var filteredCodes = await FilterVisaCodesByQualifyingAnswerAsync(currentQuestion, answer, candidateVisaCodes);

        if (filteredCodes.Count == 0)
        {
            return new EnhancedInterviewResponse
            {
                QuestionType = "NO_MATCH",
                Question = "Based on your answers, none of the visa types are suitable. Please consult with an immigration attorney for alternative options.",
                NextStep = "COMPLETE"
            };
        }

        if (filteredCodes.Count == 1)
        {
            // Perfect - exactly one match
            return await GenerateFinalRecommendationAsync(request.UserId, filteredCodes[0]);
        }

        // Still multiple matches - ask another qualifying question
        var nextQuestion = await GetMostDiscriminatingQuestionAsync(filteredCodes);
        
        if (nextQuestion == currentQuestion || string.IsNullOrEmpty(nextQuestion))
        {
            // No more discriminating questions - pick the most suitable one based on confidence score
            var bestMatch = await GetBestMatchByConfidenceAsync(filteredCodes);
            return await GenerateFinalRecommendationAsync(request.UserId, bestMatch);
        }

        return new EnhancedInterviewResponse
        {
            QuestionType = "QUALIFYING_QUESTIONS",
            Question = nextQuestion,
            Options = new List<EnhancedInterviewOption>
            {
                new() { Value = "YES", Label = "Yes" },
                new() { Value = "NO", Label = "No" }
            },
            NextStep = "DISQUALIFYING_CHECK",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = request.UserId,
                ["candidateVisaCodes"] = filteredCodes,
                ["currentQuestion"] = nextQuestion
            }
        };
    }

    private async Task<EnhancedInterviewResponse> GenerateFinalRecommendationAsync(Guid userId, string visaCode)
    {
        var visa = await _context.BaseVisaTypes
            .FirstOrDefaultAsync(v => v.Code == visaCode && v.Status == "Active");

        if (visa == null)
        {
            throw new ArgumentException($"Visa type {visaCode} not found");
        }

        return new EnhancedInterviewResponse
        {
            QuestionType = "FINAL_RECOMMENDATION",
            Question = "Based on your answers, here is the most suitable visa type for your situation:",
            FinalRecommendation = new EnhancedVisaRecommendation
            {
                Code = visa.Code,
                Name = visa.Name,
                Description = visa.Description,
                ConfidenceScore = (double)visa.ConfidenceScore,
                ReasoningPath = await GetReasoningPathAsync(userId, visaCode),
                NextSteps = await GenerateNextStepsAsync(visaCode)
            },
            NextStep = "COMPLETE",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["finalRecommendation"] = visaCode
            }
        };
    }

    // Helper methods
    private async Task<List<string>> GetVisaCodesForPurposeAndSubcategoryAsync(string purpose, string subcategory)
    {
        var mapping = _purposeMappings[purpose];
        
        // Use subcategory mappings if available
        if (mapping.SubcategoryMappings?.ContainsKey(subcategory) == true)
        {
            var patterns = mapping.SubcategoryMappings[subcategory];
            var allVisaCodes = await _context.BaseVisaTypes
                .Where(v => v.Status == "Active")
                .Select(v => v.Code)
                .ToListAsync();
                
            return allVisaCodes
                .Where(code => patterns.Any(pattern => code.StartsWith(pattern)))
                .ToList();
        }

        // Fallback to category-based mapping
        return await GetVisaCodesForPurposeAsync(purpose);
    }

    private async Task<List<string>> GetVisaCodesForPurposeAsync(string purpose)
    {
        return await _context.CategoryClasses
            .Where(c => c.IsActive && c.GeneralCategory != null && c.GeneralCategory.Contains(purpose))
            .Join(_context.BaseVisaTypes.Where(v => v.Status == "Active"),
                  cc => cc.ClassCode,
                  bv => ExtractClassCode(bv.Code),
                  (cc, bv) => bv.Code)
            .Distinct()
            .ToListAsync();
    }

    private async Task<string> GetMostDiscriminatingQuestionAsync(List<string> visaCodes)
    {
        var visaTypes = await _context.BaseVisaTypes
            .Where(v => visaCodes.Contains(v.Code) && v.Status == "Active")
            .ToListAsync();

        var questionFrequency = new Dictionary<string, int>();
        
        foreach (var visa in visaTypes)
        {
            var questions = new[] { visa.Question1, visa.Question2, visa.Question3 }
                .Where(q => !string.IsNullOrEmpty(q))
                .ToArray();

            foreach (var question in questions.Where(q => q != null))
            {
                questionFrequency[question] = questionFrequency.GetValueOrDefault(question, 0) + 1;
            }
        }

        // Return question that appears in roughly half the visa types (most discriminating)
        var targetFrequency = visaCodes.Count / 2.0;
        return questionFrequency
            .OrderBy(kv => Math.Abs(kv.Value - targetFrequency))
            .FirstOrDefault().Key ?? "";
    }

    private async Task<List<string>> FilterVisaCodesByQualifyingAnswerAsync(string question, string answer, List<string> currentCodes)
    {
        var visaTypes = await _context.BaseVisaTypes
            .Where(v => currentCodes.Contains(v.Code) && v.Status == "Active")
            .ToListAsync();

        if (answer == "YES")
        {
            // Keep visas that have this question (implies requirement)
            return visaTypes
                .Where(v => v.Question1 == question || v.Question2 == question || v.Question3 == question)
                .Select(v => v.Code)
                .ToList();
        }
        else // "NO"
        {
            // Remove visas that require this condition
            return visaTypes
                .Where(v => v.Question1 != question && v.Question2 != question && v.Question3 != question)
                .Select(v => v.Code)
                .ToList();
        }
    }

    private async Task<string> GetBestMatchByConfidenceAsync(List<string> visaCodes)
    {
        var bestVisa = await _context.BaseVisaTypes
            .Where(v => visaCodes.Contains(v.Code) && v.Status == "Active")
            .OrderByDescending(v => v.ConfidenceScore)
            .FirstOrDefaultAsync();
            
        return bestVisa?.Code ?? visaCodes.First();
    }

    private async Task<int> GetVisaCountForSubcategoryAsync(string purpose, string subcategory)
    {
        var codes = await GetVisaCodesForPurposeAndSubcategoryAsync(purpose, subcategory);
        return codes.Count;
    }

    private List<string> GetDisqualifyingQuestionsForPurpose(string purpose)
    {
        if (!_purposeMappings.ContainsKey(purpose))
            return new List<string>();
            
        return _purposeMappings[purpose].DisqualifyingAnswers.Keys.ToList();
    }

    private async Task<List<string>> GetAlternativePurposesAsync(string disqualifiedPurpose, string answer)
    {
        // Logic to suggest alternative purposes based on disqualifying answer
        return disqualifiedPurpose switch
        {
            "Tourism & Visit" when answer == "YES" => new List<string> { "Business & Employment", "Study & Exchange" },
            "Study & Exchange" when answer == "NO" => new List<string> { "Tourism & Visit", "Business & Employment" },
            "Business & Employment" when answer == "NO" => new List<string> { "Tourism & Visit", "Study & Exchange" },
            _ => _purposeMappings.Keys.Where(k => k != disqualifiedPurpose).ToList()
        };
    }

    private async Task<List<string>> GetReasoningPathAsync(Guid userId, string visaCode)
    {
        // Generate explanation of how we arrived at this recommendation
        return new List<string>
        {
            "Purpose classification matched your stated intent",
            "Subcategory selection narrowed to relevant visa types", 
            "Qualifying questions confirmed eligibility requirements",
            $"Final recommendation: {visaCode} is the most suitable option"
        };
    }

    private async Task<List<string>> GenerateNextStepsAsync(string visaCode)
    {
        // Generate next steps based on visa type
        return new List<string>
        {
            "Consult with qualified immigration attorney",
            "Gather required supporting documents",
            "Complete and file appropriate forms",
            "Schedule required interviews or appointments",
            "Monitor application status and respond to requests"
        };
    }

    private string ExtractClassCode(string visaCode)
    {
        var dashIndex = visaCode.IndexOf('-');
        if (dashIndex > 0)
        {
            return visaCode.Substring(0, dashIndex);
        }
        return visaCode.Length > 2 ? visaCode.Substring(0, 2) : visaCode;
    }

    private string GetPurposeDescription(string purpose)
    {
        return purpose switch
        {
            "Tourism & Visit" => "Tourism, business visits, or medical treatment",
            "Study & Exchange" => "Education, training, or cultural exchange programs",
            "Business & Employment" => "Work, business activities, or employment-based purposes",
            "Immigrate" => "Permanent residence in the United States",
            _ => purpose
        };
    }

    private string GetSubcategoryDescription(string subcategory)
    {
        return subcategory switch
        {
            "TOURISM" => "Tourism and sightseeing",
            "BUSINESS_VISIT" => "Business meetings, conferences, or consultations",
            "MEDICAL_TREATMENT" => "Medical treatment or procedures",
            "TRANSIT" => "Transit through the United States",
            "ACADEMIC_STUDY" => "University or college degree programs",
            "VOCATIONAL_STUDY" => "Vocational or technical training",
            "EXCHANGE_PROGRAM" => "Cultural or professional exchange programs",
            "SPECIALTY_WORKER" => "Professional work requiring specialized skills",
            "TEMPORARY_WORKER" => "Temporary or seasonal employment",
            "INTRACOMPANY_TRANSFER" => "Transfer within the same company",
            "TREATY_BUSINESS" => "Business or investment from treaty countries",
            "EXTRAORDINARY_ABILITY" => "Extraordinary ability in your field",
            "FAMILY_BASED" => "Based on family relationship with U.S. citizen/resident",
            "EMPLOYMENT_BASED" => "Based on permanent job offer or exceptional skills",
            "DIVERSITY_LOTTERY" => "Diversity visa lottery winner",
            "SPECIAL_CIRCUMSTANCES" => "Refugee, asylee, or other special situations",
            _ => subcategory
        };
    }
}

// Enhanced supporting classes
public class EnhancedInterviewRequest
{
    public Guid UserId { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class EnhancedInterviewResponse
{
    public string QuestionType { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<EnhancedInterviewOption> Options { get; set; } = new();
    public List<string> AlternativePurposes { get; set; } = new();
    public EnhancedVisaRecommendation? FinalRecommendation { get; set; }
    public string NextStep { get; set; } = string.Empty;
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class EnhancedInterviewOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<string> DisqualifyingQuestions { get; set; } = new();
    public int EstimatedVisaCount { get; set; }
}

public class EnhancedVisaRecommendation
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> ReasoningPath { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
}

public class CategoryPurposeMapping
{
    public string PrimaryPurpose { get; set; } = string.Empty;
    public string[] RequiredSubcategories { get; set; } = Array.Empty<string>();
    public Dictionary<string, string[]>? SubcategoryMappings { get; set; }
    public Dictionary<string, string[]> DisqualifyingAnswers { get; set; } = new();
}

