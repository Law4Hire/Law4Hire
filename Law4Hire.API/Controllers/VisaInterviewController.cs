using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Linq;
using System.Text.Json;

namespace Law4Hire.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisaInterviewController : ControllerBase
    {
        private readonly Law4HireDbContext _db;
        private readonly VisaInterviewBot _bot;
        private readonly WorkflowProcessingService _workflowProcessor;
        private readonly VisaEligibilityService _eligibilityService;

        public VisaInterviewController(Law4HireDbContext db, VisaInterviewBot bot, WorkflowProcessingService workflowProcessor, VisaEligibilityService eligibilityService)
        {
            _db = db;
            _bot = bot;
            _workflowProcessor = workflowProcessor;
            _eligibilityService = eligibilityService;
        }

        private string GetCurrentLanguageCode()
        {
            // Get language from Accept-Language header or default to en-US
            var acceptLanguage = Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                // Parse the first language from Accept-Language header
                var languages = acceptLanguage.Split(',');
                if (languages.Length > 0)
                {
                    var primaryLanguage = languages[0].Split(';')[0].Trim();
                    return primaryLanguage;
                }
            }
            return "en-US";
        }

        [HttpPost("step")]
        public async Task<ActionResult<VisaInterviewState>> ProcessVisaInterviewStep([FromBody] JsonElement input)
        {
            Guid userId;
            if (!input.TryGetProperty("userId", out var userIdProp) || !Guid.TryParse(userIdProp.GetString(), out userId))
            {
                return BadRequest("A valid userId (GUID) is required.");
            }

            var state = await _db.VisaInterviewStates.FirstOrDefaultAsync(s => s.UserId == userId);
            if (state == null)
            {
                state = new VisaInterviewState
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _db.VisaInterviewStates.Add(state);
            }

            string inputJson = input.ValueKind switch
            {
                JsonValueKind.String => input.GetString() ?? "\"\"",
                JsonValueKind.Array or JsonValueKind.Object => input.GetRawText(),
                _ => throw new InvalidOperationException("Invalid input format.")
            };

            var responseText = await _bot.ProcessAsync(inputJson);
            if (string.IsNullOrWhiteSpace(responseText))
                return BadRequest("Empty response from bot.");

            state.LastClientMessage = inputJson;
            state.LastBotMessage = responseText;
            state.LastUpdated = DateTime.UtcNow;

            try
            {
                if (responseText.Trim().StartsWith("["))
                {
                    using var doc = JsonDocument.Parse(responseText);
                    var root = doc.RootElement;

                    if (root.GetArrayLength() == 1)
                    {
                        state.SelectedVisaType = root[0].GetString();
                        state.CurrentVisaOptionsJson = null;
                    }
                    else
                    {
                        state.CurrentVisaOptionsJson = responseText;
                        state.SelectedVisaType = null;
                    }
                }
                else if (responseText.Trim().StartsWith("{"))
                {
                    state.VisaWorkflowJson = responseText;

                    using var doc = JsonDocument.Parse(responseText);
                    if (doc.RootElement.TryGetProperty("steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
                    {
                        var documents = new HashSet<string>();
                        foreach (var step in steps.EnumerateArray())
                        {
                            if (step.TryGetProperty("documents", out var docs) && docs.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var docItem in docs.EnumerateArray())
                                    documents.Add(docItem.GetString() ?? "");
                            }
                        }

                        state.ExtractedDocumentsJson = JsonSerializer.Serialize(documents.Where(d => !string.IsNullOrWhiteSpace(d)));
                    }
                }
                else if (responseText.Trim().StartsWith("\""))
                {
                    state.CurrentVisaOptionsJson = null;
                    state.SelectedVisaType = null;
                    state.VisaWorkflowJson = null;
                    state.ExtractedDocumentsJson = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bot response could not be parsed: " + ex.Message);
                return BadRequest("Bot returned invalid JSON.");
            }

            await _db.SaveChangesAsync();
            return Ok(state);
        }

        [HttpDelete("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetInterview([FromQuery] Guid? userId)
        {
            Guid id;
            if (userId.HasValue)
            {
                id = userId.Value;
            }
            else if (User.Identity?.IsAuthenticated == true)
            {
                id = GetUserId();
            }
            else
            {
                return BadRequest();
            }
            var existing = await _db.VisaInterviewStates.FirstOrDefaultAsync(s => s.UserId == id);
            if (existing != null)
            {
                _db.VisaInterviewStates.Remove(existing);
                await _db.SaveChangesAsync();
            }
            return NoContent();
        }

        [HttpGet("{userId:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<VisaInterviewState>> GetState(Guid userId)
        {
            var state = await _db.VisaInterviewStates.FirstOrDefaultAsync(s => s.UserId == userId);
            if (state == null)
                return NotFound();
            return Ok(state);
        }

        private static List<DocumentItem> ExtractDocuments(JsonElement workflow)
        {
            var list = new List<DocumentItem>();
            var steps = workflow.EnumerateArray().ToList();
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step.TryGetProperty("form", out var form))
                {
                    var name = form.GetProperty("name").GetString() ?? string.Empty;
                    var link = form.TryGetProperty("link", out var l) ? l.GetString() : null;
                    list.Add(new DocumentItem(name, link, i + 1));
                }
            }
            return list;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost("phase2/step")]
        public async Task<ActionResult<Phase2QuestionDto>> Phase2Step([FromBody] Phase2StepDto dto)
        {
            // 1. Get or create interview state
            var state = await _db.VisaInterviewStates.FirstOrDefaultAsync(s => s.UserId == dto.UserId);
            if (state == null)
            {
                state = new VisaInterviewState
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    LastUpdated = DateTime.UtcNow,
                    IsReset = false,
                    IsCompleted = false,
                    CurrentStep = 0
                };
                _db.VisaInterviewStates.Add(state);
                await _db.SaveChangesAsync();
                Console.WriteLine($"[DEBUG] Created new interview state for user {dto.UserId}");
            }

            Console.WriteLine($"[DEBUG] Current state - Step: {state.CurrentStep}, HasOptions: {!string.IsNullOrEmpty(state.CurrentVisaOptionsJson)}, Answer: '{dto.Answer}'");

            // Handle initial request (no visa options in state)
            if (string.IsNullOrEmpty(state.CurrentVisaOptionsJson))
            {
                Console.WriteLine("[DEBUG] INITIAL REQUEST - Setting up visa options");
                
                // Get visa types for the category
                var eligibleVisaTypes = new List<string> { "EB-1", "EB-2", "EB-3", "EB-5", "IR-1", "IR-5", "F-1", "F-2", "F-3", "F-4" };
                
                // Store visa options and increment step
                state.CurrentVisaOptionsJson = JsonSerializer.Serialize(eligibleVisaTypes);
                state.CurrentStep = 1;
                state.LastUpdated = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] Initial state saved - Step: {state.CurrentStep}, Options: {eligibleVisaTypes.Count}");

                return Ok(new Phase2QuestionDto
                {
                    Question = "What is the basis for your immigration to the United States?",
                    Options = new List<QuestionOptionDto>
                    {
                        new() { Key = "A", Text = "Family relationships (spouse, parent, child)" },
                        new() { Key = "B", Text = "Employment opportunities (job offer, special skills)" },
                        new() { Key = "C", Text = "Investment in US business" }
                    },
                    Step = state.CurrentStep,
                    IsComplete = false
                });
            }

            // Handle answer processing (visa options exist, answer provided)
            if (!string.IsNullOrEmpty(dto.Answer))
            {
                Console.WriteLine($"[DEBUG] PROCESSING ANSWER: '{dto.Answer}' for step {state.CurrentStep}");

                // Deserialize current visa list
                var currentList = JsonSerializer.Deserialize<List<string>>(state.CurrentVisaOptionsJson!);
                Console.WriteLine($"[DEBUG] Current visa list: {string.Join(", ", currentList ?? new List<string>())}");

                // Process the answer with the bot
                var answerObj = new
                {
                    Answer = dto.Answer,
                    VisaTypes = currentList,
                    Category = dto.Category
                };
                
                var botResponse = await _bot.ProcessAsync(JsonSerializer.Serialize(answerObj));
                Console.WriteLine($"[DEBUG] Bot response: {botResponse}");

                // Parse bot response
                using var doc = JsonDocument.Parse(botResponse);
                if (doc.RootElement.TryGetProperty("Payload", out var payloadElement))
                {
                    // Bot returned filtered visa list
                    var newList = JsonSerializer.Deserialize<List<string>>(payloadElement.GetRawText());
                    Console.WriteLine($"[DEBUG] Bot filtered to {newList?.Count ?? 0} visas: {string.Join(", ", newList ?? new List<string>())}");

                    if (newList != null && newList.Any())
                    {
                        // Update state with new list and increment step
                        state.CurrentVisaOptionsJson = JsonSerializer.Serialize(newList);
                        state.CurrentStep += 1;
                        state.LastUpdated = DateTime.UtcNow;
                        await _db.SaveChangesAsync();

                        Console.WriteLine($"[DEBUG] STEP PROGRESSED - New step: {state.CurrentStep}");

                        // Return next question
                        return Ok(new Phase2QuestionDto
                        {
                            Question = $"Great! Based on your answer, I've narrowed it down to {newList.Count} visa options. What best describes your specific situation?",
                            Options = new List<QuestionOptionDto>
                            {
                                new() { Key = "A", Text = "I have a direct family member who is a US citizen" },
                                new() { Key = "B", Text = "I have specific job skills or a job offer" },
                                new() { Key = "C", Text = "Other circumstances apply to me" }
                            },
                            Step = state.CurrentStep,
                            IsComplete = false
                        });
                    }
                }

                // Fallback if bot didn't return expected format
                Console.WriteLine("[WARNING] Bot didn't return expected Payload format - using fallback");
            }

            // Fallback - return current question
            Console.WriteLine("[DEBUG] FALLBACK - Returning current question");
            return Ok(new Phase2QuestionDto
            {
                Question = "I need more information to help you. Could you please provide more details?",
                Options = new List<QuestionOptionDto>(),
                Step = state.CurrentStep,
                IsComplete = false
            });
        }
}
