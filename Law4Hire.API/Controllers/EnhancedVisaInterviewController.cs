using Law4Hire.Application.Services;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnhancedVisaInterviewController : ControllerBase
    {
        private readonly EnhancedVisaInterviewService _interviewService;
        private readonly Law4HireDbContext _context;
        private readonly ILogger<EnhancedVisaInterviewController> _logger;

        public EnhancedVisaInterviewController(
            EnhancedVisaInterviewService interviewService,
            Law4HireDbContext context,
            ILogger<EnhancedVisaInterviewController> logger)
        {
            _interviewService = interviewService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Start a new enhanced visa interview session with purpose classification
        /// </summary>
        [HttpPost("start")]
        [AllowAnonymous]
        public async Task<ActionResult<EnhancedInterviewResponse>> StartEnhancedInterview([FromBody] StartEnhancedInterviewRequest request)
        {
            try
            {
                var response = await _interviewService.StartInterviewAsync(request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting enhanced interview for user {UserId}", request.UserId);
                return StatusCode(500, $"Error starting enhanced interview: {ex.Message}");
            }
        }

        /// <summary>
        /// Process an answer in the enhanced interview flow
        /// </summary>
        [HttpPost("answer")]
        [AllowAnonymous]
        public async Task<ActionResult<EnhancedInterviewResponse>> ProcessEnhancedAnswer([FromBody] EnhancedInterviewRequest request)
        {
            try
            {
                var response = await _interviewService.ProcessAnswerAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for enhanced interview");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing enhanced answer for user {UserId}", request.UserId);
                return StatusCode(500, $"Error processing enhanced answer: {ex.Message}");
            }
        }

        /// <summary>
        /// Get available purposes for the enhanced interview
        /// </summary>
        [HttpGet("purposes")]
        [AllowAnonymous]
        public ActionResult<List<EnhancedPurposeInfo>> GetAvailablePurposes()
        {
            try
            {
                var purposes = new List<EnhancedPurposeInfo>
                {
                    new() { 
                        Purpose = "Tourism & Visit", 
                        Description = "Tourism, business visits, or medical treatment",
                        EstimatedVisaTypes = 8,
                        RequiredDocuments = new[] { "Valid passport", "Financial proof", "Travel itinerary" }
                    },
                    new() { 
                        Purpose = "Study & Exchange", 
                        Description = "Education, training, or cultural exchange programs",
                        EstimatedVisaTypes = 6,
                        RequiredDocuments = new[] { "I-20 form", "Financial proof", "Academic records" }
                    },
                    new() { 
                        Purpose = "Business & Employment", 
                        Description = "Work, business activities, or employment-based purposes",
                        EstimatedVisaTypes = 15,
                        RequiredDocuments = new[] { "Job offer letter", "Labor certification", "Educational credentials" }
                    },
                    new() { 
                        Purpose = "Immigrate", 
                        Description = "Permanent residence in the United States",
                        EstimatedVisaTypes = 12,
                        RequiredDocuments = new[] { "Family/employment petition", "Medical examination", "Financial documents" }
                    }
                };

                return Ok(purposes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purposes");
                return StatusCode(500, $"Error retrieving purposes: {ex.Message}");
            }
        }

        /// <summary>
        /// Test the enhanced interview flow with simulated responses
        /// </summary>
        [HttpPost("test-enhanced-flow")]
        [AllowAnonymous]
        public async Task<ActionResult<EnhancedTestFlowResult>> TestEnhancedFlow([FromBody] EnhancedTestFlowRequest request)
        {
            try
            {
                var results = new List<string>();
                var currentResponse = await _interviewService.StartInterviewAsync(request.UserId);
                results.Add($"Started: {currentResponse.Question}");

                var testAnswers = request.TestAnswers ?? new List<string> { "Business & Employment", "NO", "SPECIALTY_WORKER", "YES" };
                var stepCount = 0;
                const int maxSteps = 15;

                foreach (var answer in testAnswers)
                {
                    if (stepCount++ >= maxSteps || currentResponse.NextStep == "COMPLETE")
                        break;

                    var answerRequest = new EnhancedInterviewRequest
                    {
                        UserId = request.UserId,
                        CurrentStep = currentResponse.NextStep,
                        Answer = answer,
                        SessionData = currentResponse.SessionData
                    };

                    currentResponse = await _interviewService.ProcessAnswerAsync(answerRequest);
                    results.Add($"Step {stepCount}: Answer '{answer}' -> {currentResponse.QuestionType}: {currentResponse.Question}");

                    if (currentResponse.FinalRecommendation != null)
                    {
                        results.Add($"Final Recommendation: {currentResponse.FinalRecommendation.Code} - {currentResponse.FinalRecommendation.Name}");
                        results.Add($"Confidence: {currentResponse.FinalRecommendation.ConfidenceScore:P1}");
                        break;
                    }

                    if (currentResponse.AlternativePurposes?.Any() == true)
                    {
                        results.Add($"Alternative purposes suggested: {string.Join(", ", currentResponse.AlternativePurposes)}");
                    }
                }

                return Ok(new EnhancedTestFlowResult
                {
                    StepCount = stepCount,
                    FinalStep = currentResponse.NextStep,
                    FinalRecommendation = currentResponse.FinalRecommendation,
                    FlowTrace = results,
                    Success = currentResponse.FinalRecommendation != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing enhanced flow");
                return StatusCode(500, $"Error testing enhanced flow: {ex.Message}");
            }
        }

        /// <summary>
        /// Get detailed visa information by code
        /// </summary>
        [HttpGet("visa/{visaCode}/details")]
        [AllowAnonymous]
        public async Task<ActionResult<EnhancedVisaDetail>> GetEnhancedVisaDetails(string visaCode)
        {
            try
            {
                var visa = await _context.BaseVisaTypes
                    .Where(v => v.Code == visaCode && v.Status == "Active")
                    .FirstOrDefaultAsync();

                if (visa == null)
                {
                    return NotFound($"Visa type {visaCode} not found");
                }

                var detail = new EnhancedVisaDetail
                {
                    Code = visa.Code,
                    Name = visa.Name,
                    Description = visa.Description,
                    QualifyingQuestions = new List<string>
                    {
                        visa.Question1,
                        visa.Question2,
                        visa.Question3
                    }.Where(q => !string.IsNullOrEmpty(q)).ToList(),
                    ConfidenceScore = (double)visa.ConfidenceScore,
                    EstimatedProcessingTime = GetEstimatedProcessingTime(visa.Code),
                    EstimatedCost = GetEstimatedCost(visa.Code),
                    RequiredDocuments = GetRequiredDocuments(visa.Code)
                };

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enhanced visa details for {VisaCode}", visaCode);
                return StatusCode(500, $"Error retrieving visa details: {ex.Message}");
            }
        }

        private string GetEstimatedProcessingTime(string visaCode)
        {
            return visaCode switch
            {
                var code when code.StartsWith("H-") => "3-6 months",
                var code when code.StartsWith("F-") => "2-4 months", 
                var code when code.StartsWith("B-") => "2-8 weeks",
                var code when code.StartsWith("EB-") => "1-3 years",
                var code when code.StartsWith("IR-") => "8-15 months",
                _ => "3-6 months"
            };
        }

        private string GetEstimatedCost(string visaCode)
        {
            return visaCode switch
            {
                var code when code.StartsWith("H-") => "$2,500 - $5,000",
                var code when code.StartsWith("F-") => "$1,000 - $2,500",
                var code when code.StartsWith("B-") => "$300 - $800",
                var code when code.StartsWith("EB-") => "$5,000 - $15,000",
                var code when code.StartsWith("IR-") => "$2,000 - $8,000",
                _ => "$1,500 - $4,000"
            };
        }

        private List<string> GetRequiredDocuments(string visaCode)
        {
            return visaCode switch
            {
                var code when code.StartsWith("H-") => new List<string> 
                { 
                    "Valid passport", "I-129 petition", "Labor condition application", 
                    "Educational credentials", "Job offer letter" 
                },
                var code when code.StartsWith("F-") => new List<string> 
                { 
                    "Valid passport", "I-20 form", "Financial proof", 
                    "Academic records", "SEVIS fee receipt" 
                },
                var code when code.StartsWith("B-") => new List<string> 
                { 
                    "Valid passport", "Financial proof", "Travel itinerary", 
                    "Purpose documentation", "Ties to home country proof" 
                },
                _ => new List<string> { "Valid passport", "Supporting documentation", "Financial proof" }
            };
        }
    }

    // Supporting DTOs for the enhanced API
    public class StartEnhancedInterviewRequest
    {
        public Guid UserId { get; set; }
    }

    public class EnhancedPurposeInfo
    {
        public string Purpose { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EstimatedVisaTypes { get; set; }
        public string[] RequiredDocuments { get; set; } = Array.Empty<string>();
    }

    public class EnhancedTestFlowRequest
    {
        public Guid UserId { get; set; }
        public List<string>? TestAnswers { get; set; }
    }

    public class EnhancedTestFlowResult
    {
        public int StepCount { get; set; }
        public string FinalStep { get; set; } = string.Empty;
        public EnhancedVisaRecommendation? FinalRecommendation { get; set; }
        public List<string> FlowTrace { get; set; } = new();
        public bool Success { get; set; }
    }

    public class EnhancedVisaDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> QualifyingQuestions { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public string EstimatedProcessingTime { get; set; } = string.Empty;
        public string EstimatedCost { get; set; } = string.Empty;
        public List<string> RequiredDocuments { get; set; } = new();
    }
}