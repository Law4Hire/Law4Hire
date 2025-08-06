using Law4Hire.Application.Services;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Law4Hire.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaticVisaInterviewController : ControllerBase
    {
        private readonly StaticVisaInterviewService _interviewService;
        private readonly Law4HireDbContext _context;

        public StaticVisaInterviewController(
            StaticVisaInterviewService interviewService,
            Law4HireDbContext context)
        {
            _interviewService = interviewService;
            _context = context;
        }

        /// <summary>
        /// Start a new static visa interview session
        /// </summary>
        [HttpPost("start")]
        [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
        public async Task<ActionResult<VisaInterviewResponse>> StartInterview([FromBody] StartInterviewRequest request)
        {
            try
            {
                var response = await _interviewService.StartInterviewAsync(request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error starting interview: {ex.Message}");
            }
        }

        /// <summary>
        /// Process an answer and get the next question or result
        /// </summary>
        [HttpPost("answer")]
        [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
        public async Task<ActionResult<VisaInterviewResponse>> ProcessAnswer([FromBody] InterviewAnswerRequest request)
        {
            try
            {
                var response = await _interviewService.ProcessAnswerAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing answer: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all available visa categories for initial classification
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CategoryInfo>>> GetVisaCategories()
        {
            try
            {
                var categories = await _context.CategoryClasses
                    .Where(c => c.IsActive)
                    .GroupBy(c => c.GeneralCategory)
                    .Select(g => new CategoryInfo
                    {
                        Category = g.Key ?? "",
                        Description = g.Key ?? "",
                        VisaClassCount = g.Count()
                    })
                    .Where(c => !string.IsNullOrEmpty(c.Category))
                    .OrderBy(c => c.Category)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving categories: {ex.Message}");
            }
        }

        /// <summary>
        /// Get details about a specific visa type
        /// </summary>
        [HttpGet("visa/{visaCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<VisaTypeDetail>> GetVisaDetails(string visaCode)
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

                var detail = new VisaTypeDetail
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
                    ConfidenceScore = (double)visa.ConfidenceScore
                };

                return Ok(detail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving visa details: {ex.Message}");
            }
        }

        /// <summary>
        /// Get statistics about the interview system
        /// </summary>
        [HttpGet("stats")]
        [AllowAnonymous]
        public async Task<ActionResult<InterviewStats>> GetInterviewStats()
        {
            try
            {
                var stats = new InterviewStats
                {
                    TotalVisaTypes = await _context.BaseVisaTypes.CountAsync(v => v.Status == "Active"),
                    TotalCategories = await _context.CategoryClasses.CountAsync(c => c.IsActive),
                    VisaTypesWithQuestions = await _context.BaseVisaTypes
                        .CountAsync(v => v.Status == "Active" && 
                                   (!string.IsNullOrEmpty(v.Question1) || 
                                    !string.IsNullOrEmpty(v.Question2) || 
                                    !string.IsNullOrEmpty(v.Question3))),
                    CategoryBreakdown = await _context.CategoryClasses
                        .Where(c => c.IsActive)
                        .GroupBy(c => c.GeneralCategory)
                        .Select(g => new CategoryStat
                        {
                            Category = g.Key ?? "Unknown",
                            Count = g.Count()
                        })
                        .OrderByDescending(cs => cs.Count)
                        .ToListAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving stats: {ex.Message}");
            }
        }

        /// <summary>
        /// Test endpoint to validate the static interview flow
        /// </summary>
        [HttpPost("test-flow")]
        [AllowAnonymous]
        public async Task<ActionResult<TestFlowResult>> TestInterviewFlow([FromBody] TestFlowRequest request)
        {
            try
            {
                var results = new List<string>();
                var currentResponse = await _interviewService.StartInterviewAsync(request.UserId);
                results.Add($"Started: {currentResponse.Question}");

                // Simulate a flow with predefined answers
                var testAnswers = request.TestAnswers ?? new List<string> { "Business", "SPECIALTY_WORKER", "YES" };
                var stepCount = 0;
                const int maxSteps = 10; // Prevent infinite loops

                foreach (var answer in testAnswers)
                {
                    if (stepCount++ >= maxSteps || currentResponse.NextStep == "COMPLETE")
                        break;

                    var answerRequest = new InterviewAnswerRequest
                    {
                        UserId = request.UserId,
                        CurrentStep = currentResponse.NextStep,
                        Answer = answer,
                        SessionData = currentResponse.SessionData
                    };

                    currentResponse = await _interviewService.ProcessAnswerAsync(answerRequest);
                    results.Add($"Step {stepCount}: Answer '{answer}' -> {currentResponse.Question}");

                    if (currentResponse.VisaRecommendations?.Any() == true)
                    {
                        results.Add($"Recommendations: {string.Join(", ", currentResponse.VisaRecommendations.Select(r => r.Code))}");
                    }
                }

                return Ok(new TestFlowResult
                {
                    StepCount = stepCount,
                    FinalStep = currentResponse.NextStep,
                    FinalRecommendations = currentResponse.VisaRecommendations?.Select(r => r.Code).ToList() ?? new List<string>(),
                    FlowTrace = results,
                    Success = currentResponse.NextStep == "COMPLETE" || currentResponse.VisaRecommendations?.Any() == true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error testing flow: {ex.Message}");
            }
        }
    }

    // Supporting DTOs for the API
    public class StartInterviewRequest
    {
        public Guid UserId { get; set; }
    }

    public class CategoryInfo
    {
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int VisaClassCount { get; set; }
    }

    public class VisaTypeDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> QualifyingQuestions { get; set; } = new();
        public double ConfidenceScore { get; set; }
    }

    public class InterviewStats
    {
        public int TotalVisaTypes { get; set; }
        public int TotalCategories { get; set; }
        public int VisaTypesWithQuestions { get; set; }
        public List<CategoryStat> CategoryBreakdown { get; set; } = new();
    }

    public class CategoryStat
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TestFlowRequest
    {
        public Guid UserId { get; set; }
        public List<string>? TestAnswers { get; set; }
    }

    public class TestFlowResult
    {
        public int StepCount { get; set; }
        public string FinalStep { get; set; } = string.Empty;
        public List<string> FinalRecommendations { get; set; } = new();
        public List<string> FlowTrace { get; set; } = new();
        public bool Success { get; set; }
    }
}