using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces; 
using Law4Hire.Core.DTOs; 
using Law4Hire.Core.Entities; 
using Law4Hire.Core.Enums; 
using System.ComponentModel.DataAnnotations;
namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class IntakeController(IIntakeSessionRepository intakeSessionRepository) : ControllerBase
{
    private readonly IIntakeSessionRepository _intakeSessionRepository = intakeSessionRepository;

    /// <summary>
    /// Create a new intake session
    /// </summary>
    /// <param name="createSessionDto">Intake session creation data</param>
    /// <returns>Created intake session</returns>
    [HttpPost("sessions")]
    [ProducesResponseType<IntakeSessionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntakeSessionDto>> CreateIntakeSession([FromBody] CreateIntakeSessionDto createSessionDto)
    {
        var session = new IntakeSession
        {
            UserId = createSessionDto.UserId,
            Language = createSessionDto.Language,
            Status = IntakeStatus.Started
        };
        
        var createdSession = await _intakeSessionRepository.CreateAsync(session);

        var sessionDto = new IntakeSessionDto(
            createdSession.Id,
            createdSession.UserId,
            createdSession.Status,
            createdSession.StartedAt,
            createdSession.CompletedAt,
            createdSession.Language,
            createdSession.SessionData
        );

        return CreatedAtAction(nameof(GetIntakeSession), new { id = createdSession.Id }, sessionDto);
    }

    /// <summary>
    /// Get intake session by ID
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <returns>Intake session details</returns>
    [HttpGet("sessions/{id:guid}")]
    [ProducesResponseType<IntakeSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntakeSessionDto>> GetIntakeSession(Guid id)
    {
        var session = await _intakeSessionRepository.GetByIdAsync(id);
        if (session == null)
            return NotFound($"Intake session with ID {id} not found.");

        var sessionDto = new IntakeSessionDto(
            session.Id,
            session.UserId,
            session.Status,
            session.StartedAt,
            session.CompletedAt,
            session.Language,
            session.SessionData
        );

        return Ok(sessionDto);
    }

    /// <summary>
    /// Get intake sessions for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's intake sessions</returns>
    [HttpGet("users/{userId:guid}/sessions")]
    [ProducesResponseType<IEnumerable<IntakeSessionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IntakeSessionDto>>> GetUserIntakeSessions(Guid userId)
    {
        var sessions = await _intakeSessionRepository.GetByUserIdAsync(userId);
        
        var sessionDtos = sessions.Select(s => new IntakeSessionDto(
            s.Id,
            s.UserId,
            s.Status,
            s.StartedAt,
            s.CompletedAt,
            s.Language,
            s.SessionData
        ));

        return Ok(sessionDtos);
    }

    [HttpPatch("sessions/{id:guid}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateSessionProgressDto progress)
    {
        var session = await _intakeSessionRepository.GetByIdAsync(id);
        if (session == null)
            return NotFound();

        session.SessionData = System.Text.Json.JsonSerializer.Serialize(progress);
        session.Status = IntakeStatus.InProgress;
        await _intakeSessionRepository.UpdateAsync(session);

        return Ok(new { Message = "Progress saved." });
    }
}
