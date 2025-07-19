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

        public VisaInterviewController(Law4HireDbContext db, VisaInterviewBot bot)
        {
            _db = db;
            _bot = bot;
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
    }
}
