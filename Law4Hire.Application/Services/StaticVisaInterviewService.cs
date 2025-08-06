using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Law4Hire.Application.Services;

/// <summary>
/// Static visa interview service that replaces OpenAI-dependent questioning
/// Uses qualifying questions from BaseVisaTypes to narrow down visa options
/// </summary>
public class StaticVisaInterviewService
{
    private readonly Law4HireDbContext _context;
    private readonly ILogger<StaticVisaInterviewService> _logger;

    public StaticVisaInterviewService(Law4HireDbContext context, ILogger<StaticVisaInterviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Start the visa interview process with purpose classification
    /// </summary>
    public async Task<VisaInterviewResponse> StartInterviewAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Starting static visa interview for user {UserId}", userId);

            // Get all active visa categories for initial classification
            var categories = await _context.CategoryClasses
                .Where(c => c.IsActive)
                .Select(c => c.GeneralCategory)
                .Distinct()
                .Where(gc => !string.IsNullOrEmpty(gc))
                .ToListAsync();

            var purposeOptions = categories
                .SelectMany(c => c?.Split(',').Select(s => s.Trim()) ?? Enumerable.Empty<string>())
                .Distinct()
                .OrderBy(c => c)
                .Select(category => new InterviewOption
                {
                    Value = category,
                    Label = GetCategoryDescription(category)
                })
                .ToList();

            return new VisaInterviewResponse
            {
                QuestionType = "PURPOSE_CLASSIFICATION",
                Question = "What is your primary purpose for coming to the United States?",
                Options = purposeOptions,
                NextStep = "CATEGORY_FILTERING",
                SessionData = new Dictionary<string, object>
                {
                    ["userId"] = userId,
                    ["step"] = "PURPOSE_CLASSIFICATION"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting interview for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Process user answer and continue the interview flow
    /// </summary>
    public async Task<VisaInterviewResponse> ProcessAnswerAsync(InterviewAnswerRequest request)
    {
        try
        {
            _logger.LogInformation("Processing answer for user {UserId}, step {Step}", 
                request.UserId, request.CurrentStep);

            return request.CurrentStep switch
            {
                "PURPOSE_CLASSIFICATION" => await ProcessPurposeAnswerAsync(request),
                "CATEGORY_FILTERING" => await ProcessCategoryAnswerAsync(request),
                "QUALIFYING_QUESTIONS" => await ProcessQualifyingAnswerAsync(request),
                "FINAL_SELECTION" => await ProcessFinalSelectionAsync(request),
                _ => throw new ArgumentException($"Unknown step: {request.CurrentStep}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing answer for user {UserId}", request.UserId);
            throw;
        }
    }

    private async Task<VisaInterviewResponse> ProcessPurposeAnswerAsync(InterviewAnswerRequest request)
    {
        var selectedPurpose = request.Answer;
        
        // Get visa codes that match this purpose
        var visaCodes = await GetVisaCodesForPurposeAsync(selectedPurpose);

        if (visaCodes.Count <= 5)
        {
            // Few enough options - move directly to qualifying questions
            return await GenerateQualifyingQuestionAsync(request.UserId, visaCodes);
        }

        // Too many options - ask for more specific category
        var subcategories = GetSubcategoriesForPurpose(selectedPurpose);
        
        return new VisaInterviewResponse
        {
            QuestionType = "CATEGORY_FILTERING",
            Question = $"Which of these best describes your specific situation for {selectedPurpose.ToLower()}?",
            Options = subcategories,
            NextStep = "QUALIFYING_QUESTIONS",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = request.UserId,
                ["step"] = "CATEGORY_FILTERING",
                ["selectedPurpose"] = selectedPurpose,
                ["visaCodes"] = visaCodes
            }
        };
    }

    private async Task<VisaInterviewResponse> ProcessCategoryAnswerAsync(InterviewAnswerRequest request)
    {
        var selectedCategory = request.Answer;
        var allVisaCodes = (List<string>)request.SessionData["visaCodes"];
        
        // Filter visa codes based on subcategory
        var filteredCodes = FilterVisaCodesByCategory(selectedCategory, allVisaCodes);

        if (filteredCodes.Count <= 3)
        {
            // Few enough - show final recommendations
            return await GenerateFinalRecommendationsAsync(request.UserId, filteredCodes);
        }

        // Still need more narrowing - ask qualifying questions
        return await GenerateQualifyingQuestionAsync(request.UserId, filteredCodes);
    }

    private async Task<VisaInterviewResponse> ProcessQualifyingAnswerAsync(InterviewAnswerRequest request)
    {
        var answer = request.Answer;
        var currentVisaCodes = (List<string>)request.SessionData["visaCodes"];
        var currentQuestion = (string)request.SessionData["currentQuestion"];

        // Filter visa codes based on the answer to the qualifying question
        var filteredCodes = await FilterVisaCodesByQualifyingAnswerAsync(currentQuestion, answer, currentVisaCodes);

        if (filteredCodes.Count == 0)
        {
            // No matching visas - provide guidance
            return new VisaInterviewResponse
            {
                QuestionType = "NO_MATCH",
                Question = "Based on your answers, we couldn't find a matching visa type. Please consider consulting with an immigration attorney for personalized advice.",
                NextStep = "COMPLETE",
                SessionData = new Dictionary<string, object>
                {
                    ["userId"] = request.UserId,
                    ["result"] = "NO_MATCH"
                }
            };
        }

        if (filteredCodes.Count <= 2)
        {
            // Close enough - show recommendations
            return await GenerateFinalRecommendationsAsync(request.UserId, filteredCodes);
        }

        // Still need more narrowing - ask another qualifying question
        return await GenerateQualifyingQuestionAsync(request.UserId, filteredCodes);
    }

    private async Task<VisaInterviewResponse> ProcessFinalSelectionAsync(InterviewAnswerRequest request)
    {
        var selectedVisaCode = request.Answer;
        return await GenerateWorkflowAsync(request.UserId, selectedVisaCode);
    }

    private async Task<VisaInterviewResponse> GenerateQualifyingQuestionAsync(Guid userId, List<string> visaCodes)
    {
        // Get visa types with their qualifying questions
        var visaTypes = await _context.BaseVisaTypes
            .Where(v => visaCodes.Contains(v.Code) && v.Status == "Active")
            .ToListAsync();

        // Find the most discriminating question
        var questionScores = new Dictionary<string, QuestionScore>();
        
        foreach (var visa in visaTypes)
        {
            var questions = new[] { visa.Question1, visa.Question2, visa.Question3 }
                .Where(q => !string.IsNullOrEmpty(q))
                .ToArray();

            foreach (var question in questions)
            {
                if (question != null && !questionScores.ContainsKey(question))
                {
                    questionScores[question] = new QuestionScore { Question = question };
                }
                if (question != null)
                {
                    questionScores[question].VisaCodes.Add(visa.Code);
                    questionScores[question].Frequency++;
                }
            }
        }

        // Select question that divides the visa types most evenly
        var bestQuestion = questionScores.Values
            .Where(qs => qs.Frequency > 1) // Must apply to multiple visas
            .OrderByDescending(qs => Math.Min(qs.Frequency, visaCodes.Count - qs.Frequency))
            .FirstOrDefault();

        if (bestQuestion == null)
        {
            // No good discriminating questions - show final options
            return await GenerateFinalRecommendationsAsync(userId, visaCodes);
        }

        return new VisaInterviewResponse
        {
            QuestionType = "QUALIFYING_QUESTIONS",
            Question = bestQuestion.Question,
            Options = new List<InterviewOption>
            {
                new() { Value = "YES", Label = "Yes" },
                new() { Value = "NO", Label = "No" },
                new() { Value = "UNSURE", Label = "I'm not sure" }
            },
            NextStep = "QUALIFYING_QUESTIONS",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["step"] = "QUALIFYING_QUESTIONS",
                ["visaCodes"] = visaCodes,
                ["currentQuestion"] = bestQuestion.Question
            }
        };
    }

    private async Task<VisaInterviewResponse> GenerateFinalRecommendationsAsync(Guid userId, List<string> visaCodes)
    {
        var recommendations = await GetVisaRecommendationsAsync(visaCodes);

        if (recommendations.Count == 1)
        {
            // Single recommendation - go directly to workflow
            return await GenerateWorkflowAsync(userId, recommendations[0].Code);
        }

        return new VisaInterviewResponse
        {
            QuestionType = "FINAL_SELECTION",
            Question = "Based on your answers, here are the most suitable visa options for you. Please select one to see the application process:",
            VisaRecommendations = recommendations,
            NextStep = "WORKFLOW_GENERATION",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["finalOptions"] = visaCodes
            }
        };
    }

    private async Task<VisaInterviewResponse> GenerateWorkflowAsync(Guid userId, string visaCode)
    {
        var visa = await _context.BaseVisaTypes
            .FirstOrDefaultAsync(v => v.Code == visaCode && v.Status == "Active");

        if (visa == null)
        {
            throw new ArgumentException($"Visa type {visaCode} not found");
        }

        // Generate a basic workflow (this would be expanded based on your workflow system)
        var workflow = new VisaWorkflow
        {
            VisaCode = visa.Code,
            VisaName = visa.Name,
            Description = visa.Description,
            Steps = GenerateBasicWorkflowSteps(visa.Code),
            EstimatedTimeMonths = EstimateProcessingTime(visa.Code),
            EstimatedCost = EstimateApplicationCost(visa.Code)
        };

        return new VisaInterviewResponse
        {
            QuestionType = "WORKFLOW_RESULT",
            Question = $"Here is the application process for {visa.Name}:",
            Workflow = workflow,
            NextStep = "COMPLETE",
            SessionData = new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["selectedVisa"] = visaCode,
                ["result"] = "WORKFLOW_GENERATED"
            }
        };
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

    private async Task<List<VisaRecommendation>> GetVisaRecommendationsAsync(List<string> visaCodes)
    {
        var visaTypes = await _context.BaseVisaTypes
            .Where(v => visaCodes.Contains(v.Code) && v.Status == "Active")
            .ToListAsync();

        return visaTypes.Select(v => new VisaRecommendation
        {
            Code = v.Code,
            Name = v.Name,
            Description = v.Description,
            ConfidenceScore = (double)v.ConfidenceScore
        }).ToList();
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
        else if (answer == "NO")
        {
            // Remove visas that require this condition
            return visaTypes
                .Where(v => v.Question1 != question && v.Question2 != question && v.Question3 != question)
                .Select(v => v.Code)
                .ToList();
        }
        else // UNSURE
        {
            // Keep all options but might add note
            return currentCodes;
        }
    }

    private List<string> FilterVisaCodesByCategory(string category, List<string> currentCodes)
    {
        // Simplified filtering based on category keywords
        var categoryMapping = new Dictionary<string, string[]>
        {
            ["SPECIALTY_WORKER"] = new[] { "H-1B", "H-1B1", "E-3" },
            ["TEMPORARY_WORKER"] = new[] { "H-2A", "H-2B", "H-3" },
            ["INTRACOMPANY_TRANSFER"] = new[] { "L-1", "L-2" },
            ["TREATY_BUSINESS"] = new[] { "E-1", "E-2" },
            ["EXTRAORDINARY_ABILITY"] = new[] { "O-1", "O-2", "O-3", "EB-1A" },
            ["ACADEMIC_STUDY"] = new[] { "F-1", "F-2" },
            ["VOCATIONAL_STUDY"] = new[] { "M-1", "M-2" },
            ["EXCHANGE_PROGRAM"] = new[] { "J-1", "J-2", "Q-1" },
            ["FAMILY_IMMEDIATE"] = new[] { "IR-", "CR-", "K-" },
            ["EMPLOYMENT_BASED"] = new[] { "EB-" },
            ["TOURISM"] = new[] { "B-1/B-2" },
            ["TRANSIT"] = new[] { "C-" }
        };

        if (categoryMapping.ContainsKey(category))
        {
            var patterns = categoryMapping[category];
            return currentCodes
                .Where(code => patterns.Any(pattern => code.StartsWith(pattern)))
                .ToList();
        }

        return currentCodes;
    }

    private List<InterviewOption> GetSubcategoriesForPurpose(string purpose)
    {
        return purpose switch
        {
            "Business" or "Employment" => new List<InterviewOption>
            {
                new() { Value = "SPECIALTY_WORKER", Label = "Professional work requiring a degree" },
                new() { Value = "TEMPORARY_WORKER", Label = "Temporary or seasonal work" },
                new() { Value = "INTRACOMPANY_TRANSFER", Label = "Transfer within the same company" },
                new() { Value = "TREATY_BUSINESS", Label = "Business/investment from treaty country" },
                new() { Value = "EXTRAORDINARY_ABILITY", Label = "Extraordinary ability in your field" }
            },
            "Study & Exchange" => new List<InterviewOption>
            {
                new() { Value = "ACADEMIC_STUDY", Label = "University or college education" },
                new() { Value = "VOCATIONAL_STUDY", Label = "Vocational or technical training" },
                new() { Value = "EXCHANGE_PROGRAM", Label = "Cultural or professional exchange" }
            },
            "Tourism & Visit" => new List<InterviewOption>
            {
                new() { Value = "TOURISM", Label = "Tourism and sightseeing" },
                new() { Value = "BUSINESS_VISIT", Label = "Business meetings or conferences" },
                new() { Value = "MEDICAL_TREATMENT", Label = "Medical treatment" },
                new() { Value = "TRANSIT", Label = "Transit through the U.S." }
            },
            "Immigrate" => new List<InterviewOption>
            {
                new() { Value = "FAMILY_IMMEDIATE", Label = "Immediate family member of U.S. citizen/resident" },
                new() { Value = "EMPLOYMENT_BASED", Label = "Permanent job offer or extraordinary skills" },
                new() { Value = "DIVERSITY_LOTTERY", Label = "Diversity visa lottery winner" },
                new() { Value = "SPECIAL_CIRCUMSTANCES", Label = "Refugee, asylee, or special immigrant" }
            },
            _ => new List<InterviewOption>()
        };
    }

    private string GetCategoryDescription(string category)
    {
        return category switch
        {
            "Business" => "Work or business activities",
            "Employment" => "Employment-based immigration",
            "Study & Exchange" => "Education or cultural exchange",
            "Tourism & Visit" => "Tourism, visits, or transit",
            "Immigrate" => "Permanent residence",
            "Other" => "Special circumstances",
            _ => category
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

    private List<InterviewWorkflowStep> GenerateBasicWorkflowSteps(string visaCode)
    {
        // This would be expanded based on your specific workflow requirements
        return new List<InterviewWorkflowStep>
        {
            new() { Name = "Initial Consultation", Description = "Meet with immigration attorney", EstimatedDays = 1 },
            new() { Name = "Document Preparation", Description = "Gather required documents", EstimatedDays = 14 },
            new() { Name = "Form Filing", Description = "Submit application forms", EstimatedDays = 3 },
            new() { Name = "Processing", Description = "USCIS/consular processing", EstimatedDays = GetProcessingDays(visaCode) },
            new() { Name = "Interview/Decision", Description = "Final interview and decision", EstimatedDays = 1 }
        };
    }

    private int GetProcessingDays(string visaCode)
    {
        // Simplified processing time estimates
        return visaCode switch
        {
            var code when code.StartsWith("H-") => 90,
            var code when code.StartsWith("F-") => 60,
            var code when code.StartsWith("B-") => 30,
            var code when code.StartsWith("EB-") => 365,
            var code when code.StartsWith("IR-") => 180,
            _ => 90
        };
    }

    private int EstimateProcessingTime(string visaCode)
    {
        return GetProcessingDays(visaCode) / 30; // Convert to months
    }

    private decimal EstimateApplicationCost(string visaCode)
    {
        // Simplified cost estimates
        return visaCode switch
        {
            var code when code.StartsWith("H-") => 2500m,
            var code when code.StartsWith("F-") => 1500m,
            var code when code.StartsWith("B-") => 500m,
            var code when code.StartsWith("EB-") => 5000m,
            var code when code.StartsWith("IR-") => 3000m,
            _ => 2000m
        };
    }
}

// Supporting classes
public class InterviewAnswerRequest
{
    public Guid UserId { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class VisaInterviewResponse
{
    public string QuestionType { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<InterviewOption> Options { get; set; } = new();
    public List<VisaRecommendation> VisaRecommendations { get; set; } = new();
    public VisaWorkflow? Workflow { get; set; }
    public string NextStep { get; set; } = string.Empty;
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class InterviewOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class VisaRecommendation
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
}

public class VisaWorkflow
{
    public string VisaCode { get; set; } = string.Empty;
    public string VisaName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<InterviewWorkflowStep> Steps { get; set; } = new();
    public int EstimatedTimeMonths { get; set; }
    public decimal EstimatedCost { get; set; }
}

public class InterviewWorkflowStep
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedDays { get; set; }
}

public class QuestionScore
{
    public string Question { get; set; } = string.Empty;
    public List<string> VisaCodes { get; set; } = new();
    public int Frequency { get; set; }
}