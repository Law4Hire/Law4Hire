using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
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

        public VisaInterviewController(Law4HireDbContext db, VisaInterviewBot bot, WorkflowProcessingService workflowProcessor)
        {
            _db = db;
            _bot = bot;
            _workflowProcessor = workflowProcessor;
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
            }

            string? botResponse = null;

            // 2. If no visa options stored, do handshake and get list, then get question
            if (string.IsNullOrEmpty(state.CurrentVisaOptionsJson))
            {
                try
                {
                    // Step 2a: Send category to get visa types list
                    var handshakePayload = new
                    {
                        category = dto.Category,
                        instructions = dto.Instructions
                    };
                    var handshakeJson = JsonSerializer.Serialize(handshakePayload);
                    Console.WriteLine($"[DEBUG] Sending handshake: {handshakeJson}");

                    var handshakeResponse = await _bot.ProcessAsync(handshakeJson);
                    Console.WriteLine($"[DEBUG] Handshake response: {handshakeResponse}");

                    // Step 2b: Parse and store the visa types list
                    List<string> visaTypeList;
                    try
                    {
                        // First try to parse as a simple array (old format)
                        visaTypeList = JsonSerializer.Deserialize<List<string>>(handshakeResponse) ?? new List<string>();
                        Console.WriteLine($"[DEBUG] Parsed as simple array: {JsonSerializer.Serialize(visaTypeList)}");
                    }
                    catch (JsonException)
                    {
                        try
                        {
                            // If that fails, try to parse as an object with visaTypes property (new format)
                            using var doc = JsonDocument.Parse(handshakeResponse);
                            if (doc.RootElement.TryGetProperty("visaTypes", out var visaTypesElement))
                            {
                                visaTypeList = JsonSerializer.Deserialize<List<string>>(visaTypesElement.GetRawText()) ?? new List<string>();
                                Console.WriteLine($"[DEBUG] Parsed from object.visaTypes: {JsonSerializer.Serialize(visaTypeList)}");
                            }
                            else
                            {
                                Console.WriteLine("[ERROR] No visaTypes property found in response");
                                visaTypeList = GetFallbackVisaTypes(dto.Category);
                            }
                        }
                        catch (JsonException ex2)
                        {
                            Console.WriteLine($"[ERROR] Failed to parse handshake response as object: {ex2.Message}");
                            visaTypeList = GetFallbackVisaTypes(dto.Category);
                        }
                    }

                    if (visaTypeList == null || !visaTypeList.Any())
                    {
                        // Fallback with default visa types based on category
                        visaTypeList = GetFallbackVisaTypes(dto.Category);
                        Console.WriteLine($"[DEBUG] Using fallback visa types: {JsonSerializer.Serialize(visaTypeList)}");
                    }

                    state.CurrentVisaOptionsJson = JsonSerializer.Serialize(visaTypeList);
                    state.LastUpdated = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    // Step 2c: Now send the list back to get the first question
                    var listPayload = JsonSerializer.Serialize(visaTypeList);
                    Console.WriteLine($"[DEBUG] Sending visa list for question: {listPayload}");

                    botResponse = await _bot.ProcessAsync(listPayload);
                    Console.WriteLine($"[DEBUG] Bot response to visa list: {botResponse}");

                    // Check if the response looks like a question
                    if (botResponse.Trim().StartsWith("["))
                    {
                        Console.WriteLine("[ERROR] Bot returned array instead of question!");
                        // Fallback question
                        botResponse = "\"What is the primary purpose of your visit to the United States?\"";
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[ERROR] JSON parsing failed: {ex.Message}");
                    return BadRequest($"Failed to parse visa types: {ex.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(dto.Answer))
            {
                // 3. Process user's answer
                try
                {
                    var currentList = JsonSerializer.Deserialize<List<string>>(state.CurrentVisaOptionsJson);
                    if (currentList == null || !currentList.Any())
                    {
                        return BadRequest("No current visa options available");
                    }

                    var payload = new
                    {
                        visaTypes = currentList,
                        answer = dto.Answer
                    };
                    var payloadJson = JsonSerializer.Serialize(payload);
                    Console.WriteLine($"[DEBUG] Sending answer payload: {payloadJson}");

                    botResponse = await _bot.ProcessAsync(payloadJson);
                    Console.WriteLine($"[DEBUG] Bot response to answer: {botResponse}");

                    // Check if response is an object with both visaTypes and question
                    if (botResponse.Trim().StartsWith("{"))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(botResponse);

                            // Extract visaTypes if present
                            if (doc.RootElement.TryGetProperty("visaTypes", out var visaTypesElement))
                            {
                                var newList = JsonSerializer.Deserialize<List<string>>(visaTypesElement.GetRawText());

                                if (newList != null && newList.Any())
                                {
                                    // Update the visa list in state
                                    state.CurrentVisaOptionsJson = JsonSerializer.Serialize(newList);
                                    state.CurrentStep += 1;
                                    state.LastUpdated = DateTime.UtcNow;
                                    await _db.SaveChangesAsync();

                                    Console.WriteLine($"[DEBUG] Updated list has {newList.Count} items: {JsonSerializer.Serialize(newList)}");

                                    // Check if down to one visa
                                    if (newList.Count == 1)
                                    {
                                        var workflowPayload = newList[0];
                                        Console.WriteLine($"[DEBUG] Requesting workflow for: {workflowPayload}");

                                        var workflowResponse = await _bot.ProcessAsync(workflowPayload);
                                        Console.WriteLine($"[DEBUG] Workflow response: {workflowResponse}");

                                        state.VisaWorkflowJson = workflowResponse;
                                        state.SelectedVisaType = newList[0];
                                        state.IsCompleted = true;
                                        state.LastUpdated = DateTime.UtcNow;
                                        await _db.SaveChangesAsync();

                                        // Process workflow documents into UserDocumentStatuses
                                        try
                                        {
                                            await _workflowProcessor.ProcessWorkflowSteps(dto.UserId, newList[0], finalWorkflow);
                                            Console.WriteLine("[DEBUG] Workflow steps processed successfully");
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[ERROR] Failed to process workflow steps: {ex.Message}");
                                            // Continue anyway - we still have the workflow saved
                                        }

                                        // Also make sure you have the finalWorkflow variable defined - add this before the try block:
                                        string finalWorkflow;
                                        try
                                        {
                                            using var testDoc = JsonDocument.Parse(workflowResponse);
                                            if (testDoc.RootElement.TryGetProperty("steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
                                            {
                                                finalWorkflow = workflowResponse;
                                                Console.WriteLine("[DEBUG] Using bot-generated workflow");
                                            }
                                            else
                                            {
                                                finalWorkflow = CreateFallbackWorkflow(workflowPayload);
                                                Console.WriteLine("[DEBUG] Bot returned bad format, using fallback workflow");
                                            }
                                        }
                                        catch (JsonException)
                                        {
                                            finalWorkflow = CreateFallbackWorkflow(workflowPayload);
                                            Console.WriteLine("[DEBUG] Bot returned invalid JSON, using fallback workflow");
                                        }


                                        return Ok(new Phase2QuestionDto
                                        {
                                            Question = $"Perfect! I recommend the {newList[0]} visa. Your personalized workflow has been created and documents have been organized for you.",
                                            Step = state.CurrentStep,
                                            IsComplete = true
                                        });
                                    }
                                }
                            }

                            // Extract question if present
                            if (doc.RootElement.TryGetProperty("question", out var questionElement))
                            {
                                botResponse = JsonSerializer.Serialize(questionElement.GetString());
                                Console.WriteLine($"[DEBUG] Extracted question: {botResponse}");
                            }
                            else
                            {
                                // No question in response, ask for clarification
                                botResponse = "\"Could you please provide more details about your specific situation?\"";
                            }
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to parse object response: {ex.Message}");
                            botResponse = "\"Could you please clarify your answer?\"";
                        }
                    }
                    // If response is a simple array (old format)
                    else if (botResponse.Trim().StartsWith("["))
                    {
                        var newList = JsonSerializer.Deserialize<List<string>>(botResponse);

                        // Handle empty array case
                        if (newList == null || !newList.Any())
                        {
                            Console.WriteLine("[ERROR] Bot returned empty array - asking for clarification");
                            botResponse = "\"I didn't quite understand your answer. Could you please clarify your specific situation?\"";
                        }
                        else if (newList.Count == currentList.Count &&
                                newList.All(currentList.Contains) &&
                                currentList.All(newList.Contains))
                        {
                            Console.WriteLine("[ERROR] Bot didn't filter the list - checking if we need to force progression");

                            // Check if we've been stuck on the same list for too many steps
                            if (state.CurrentStep >= 3)
                            {
                                Console.WriteLine("[FORCE] Too many steps without progress - forcing decision");

                                // Force a decision based on common scenarios
                                string forcedRecommendation = ForceBestVisaChoice(currentList, dto.Answer);

                                // Create a single-item list with our forced choice
                                var forcedList = new List<string> { forcedRecommendation };

                                state.CurrentVisaOptionsJson = JsonSerializer.Serialize(forcedList);
                                state.CurrentStep += 1;
                                state.LastUpdated = DateTime.UtcNow;
                                await _db.SaveChangesAsync();

                                Console.WriteLine($"[FORCE] Forced recommendation: {forcedRecommendation}");

                                // Get workflow for the forced choice
                                var workflowPayload = forcedRecommendation;
                                var workflowResponse = await _bot.ProcessAsync(workflowPayload);

                                state.VisaWorkflowJson = workflowResponse;
                                state.SelectedVisaType = forcedRecommendation;
                                state.IsCompleted = true;
                                state.LastUpdated = DateTime.UtcNow;
                                await _db.SaveChangesAsync();

                                // Process workflow documents
                                try
                                {
                                    await _workflowProcessor.ProcessWorkflowDocuments(dto.UserId, forcedRecommendation, workflowResponse);
                                    Console.WriteLine("[DEBUG] Workflow documents processed successfully");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[ERROR] Failed to process workflow documents: {ex.Message}");
                                }

                                return Ok(new Phase2QuestionDto
                                {
                                    Question = $"Based on your answers, I recommend the {forcedRecommendation} visa for your trip to Disney World. Your personalized workflow has been created!",
                                    Step = state.CurrentStep,
                                    IsComplete = true
                                });
                            }
                            else
                            {
                                // Try a different approach question
                                botResponse = GetAlternativeQuestion(currentList);
                            }
                        }
                        else
                        {
                            // List was properly filtered
                            state.CurrentVisaOptionsJson = JsonSerializer.Serialize(newList);
                            state.CurrentStep += 1;
                            state.LastUpdated = DateTime.UtcNow;
                            await _db.SaveChangesAsync();

                            Console.WriteLine($"[DEBUG] Updated list has {newList.Count} items: {JsonSerializer.Serialize(newList)}");

                            // If down to one visa, get workflow
                            if (newList.Count == 1)
                            {
                                var workflowPayload = newList[0];
                                Console.WriteLine($"[DEBUG] Requesting workflow for: {workflowPayload}");

                                var workflowResponse = await _bot.ProcessAsync(workflowPayload);
                                Console.WriteLine($"[DEBUG] Workflow response: {workflowResponse}");

                                state.VisaWorkflowJson = workflowResponse;
                                state.SelectedVisaType = newList[0];
                                state.IsCompleted = true;
                                state.LastUpdated = DateTime.UtcNow;
                                await _db.SaveChangesAsync();

                                return Ok(new Phase2QuestionDto
                                {
                                    Question = $"Perfect! I recommend the {newList[0]} visa. Your personalized workflow has been created.",
                                    Step = state.CurrentStep,
                                    IsComplete = true
                                });
                            }
                            else
                            {
                                // Get next question for the reduced list
                                var questionPayload = JsonSerializer.Serialize(newList);
                                Console.WriteLine($"[DEBUG] Getting next question for list: {questionPayload}");

                                botResponse = await _bot.ProcessAsync(questionPayload);
                                Console.WriteLine($"[DEBUG] Next question response: {botResponse}");
                            }
                        }
                    }
                    // If response is just a question string
                    else if (botResponse.Trim().StartsWith("\""))
                    {
                        // It's already a question, use as-is
                        Console.WriteLine($"[DEBUG] Bot returned question string: {botResponse}");
                    }
                    else
                    {
                        // Unknown format, ask for clarification
                        Console.WriteLine($"[ERROR] Unknown response format: {botResponse}");
                        botResponse = "\"Could you please provide more specific details about your situation?\"";
                    }
                }

                catch (JsonException ex)
                {
                    Console.WriteLine($"[ERROR] Answer processing failed: {ex.Message}");
                    return BadRequest($"Failed to process answer: {ex.Message}");
                }
            }
            else
            {
                // 4. No answer provided, return current question if available
                if (!string.IsNullOrEmpty(state.LastBotMessage))
                {
                    botResponse = state.LastBotMessage;
                    Console.WriteLine($"[DEBUG] Using stored question: {botResponse}");
                }
                else
                {
                    return BadRequest("No question available. Please restart the interview.");
                }
            }

            // 5. Handle edge cases for botResponse
            if (string.IsNullOrEmpty(botResponse) || botResponse.Trim() == "{}")
            {
                Console.WriteLine("[ERROR] Bot returned empty or invalid response");
                botResponse = "\"Could you please provide more details about your situation?\"";
            }

            // 6. Store and return the question
            state.LastBotMessage = botResponse;
            state.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // 7. Parse the question from bot response
            try
            {
                string questionText;

                // Check if it's still returning a list (error case)
                if (botResponse.Trim().StartsWith("["))
                {
                    Console.WriteLine("[ERROR] Bot response is still an array!");
                    questionText = "What is the primary purpose of your visit to the United States?";
                }
                else if (botResponse.Trim().StartsWith("\"") && botResponse.Trim().EndsWith("\""))
                {
                    // It's a JSON string, deserialize it
                    questionText = JsonSerializer.Deserialize<string>(botResponse) ?? "Could you please provide more details?";
                    Console.WriteLine($"[DEBUG] Parsed question from JSON string: {questionText}");
                }
                else
                {
                    // It's plain text, use as-is but clean it up
                    questionText = botResponse.Trim().Trim('"');
                    Console.WriteLine($"[DEBUG] Using plain text question: {questionText}");
                }

                Console.WriteLine($"[DEBUG] Final question to user: {questionText}");

                return Ok(new Phase2QuestionDto
                {
                    Question = questionText,
                    Step = state.CurrentStep,
                    IsComplete = false
                });
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[ERROR] Question parsing failed: {ex.Message}");
                // If parsing fails, return a fallback question
                return Ok(new Phase2QuestionDto
                {
                    Question = "Could you please tell me more about your specific situation?",
                    Step = state.CurrentStep,
                    IsComplete = false
                });
            }
        }

        private string ForceBestVisaChoice(List<string> currentList, string lastAnswer)
        {
            var answer = lastAnswer.ToLower();

            // Tourism/leisure scenarios
            if (answer.Contains("tourism") || answer.Contains("leisure") || answer.Contains("disney") ||
                answer.Contains("vacation") || answer.Contains("holiday"))
            {
                // For tourism, prefer ESTA if available (easier process), otherwise B-2
                if (currentList.Contains("ESTA")) return "ESTA";
                if (currentList.Contains("B-2")) return "B-2";
                if (currentList.Contains("WT")) return "WT";
            }

            // Business scenarios
            if (answer.Contains("business") || answer.Contains("meeting") || answer.Contains("conference"))
            {
                if (currentList.Contains("B-1")) return "B-1";
                if (currentList.Contains("ESTA")) return "ESTA"; // Can be used for business too
            }

            // Investment scenarios
            if (answer.Contains("invest") || answer.Contains("business"))
            {
                if (currentList.Contains("E-2")) return "E-2";
                if (currentList.Contains("EB-5")) return "EB-5";
            }

            // Work scenarios
            if (answer.Contains("work") || answer.Contains("job") || answer.Contains("employ"))
            {
                if (currentList.Contains("H-1B")) return "H-1B";
                if (currentList.Contains("L-1A")) return "L-1A";
                if (currentList.Contains("O-1A")) return "O-1A";
            }

            // Default: return the first item in the list
            return currentList.First();
        }

        // Helper method to provide fallback visa types
        private List<string> GetFallbackVisaTypes(string category)
        {
            return category.ToLower() switch
            {
                var c when c.Contains("tourism") || c.Contains("business") =>
                    new List<string> { "B-1", "B-2", "ESTA", "WT", "WB" },
                var c when c.Contains("investment") || c.Contains("business") =>
                    new List<string> { "E-1", "E-2", "EB-5", "L-1A", "L-1B", "H-1B", "O-1A" },
                var c when c.Contains("permanent") || c.Contains("green card") =>
                    new List<string> { "EB-1", "EB-2", "EB-3", "EB-4", "EB-5", "IR1", "IR2", "IR3", "IR4", "IR5", "F1", "F2A", "F2B", "F3", "F4", "DV", "SB-1" },
                var c when c.Contains("study") || c.Contains("education") =>
                    new List<string> { "F-1", "F-2", "J-1", "J-2", "M-1", "M-2" },
                var c when c.Contains("work") || c.Contains("employment") =>
                    new List<string> { "H-1B", "H-2A", "H-2B", "L-1A", "L-1B", "O-1A", "O-1B", "TN", "E-3" },
                _ => new List<string> { "B-1", "B-2", "F-1", "H-1B", "EB-5" } // Generic fallback
            };
        }

        // Helper method to provide alternative questions when filtering fails
        private string GetAlternativeQuestion(List<string> visaTypes)
        {
            if (visaTypes.Any(v => v.StartsWith("EB-")))
                return "\"Are you looking to immigrate permanently or temporarily to the United States?\"";
            if (visaTypes.Any(v => v.StartsWith("H-") || v.StartsWith("L-")))
                return "\"Do you have a job offer from a U.S. employer?\"";
            if (visaTypes.Any(v => v.StartsWith("B-") || v == "ESTA"))
                return "\"Are you planning to visit for business, tourism, or medical treatment?\"";

            return "\"Could you describe your specific situation in more detail?\"";
        }
        public class Phase2StepDto
        {
            public Guid UserId { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Instructions { get; set; } = string.Empty;
            public string? Answer { get; set; }
        }

        public class Phase2QuestionDto
        {
            public string Question { get; set; } = string.Empty;
            public int Step { get; set; }
            public bool IsComplete { get; set; }
        }
    }
}
