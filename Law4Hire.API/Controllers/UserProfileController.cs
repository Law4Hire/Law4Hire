using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly Law4HireDbContext _context;

    public UserProfileController(Law4HireDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(Guid userId)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.CitizenshipCountry)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(new UserProfileDto(
            profile.Id,
            profile.UserId,
            profile.DateOfBirth,
            profile.CitizenshipCountryId,
            profile.CitizenshipCountry?.Name,
            profile.MaritalStatus,
            profile.HasRelativesInUS,
            profile.HasJobOffer,
            profile.EducationLevel,
            profile.FearOfPersecution,
            profile.HasPastVisaDenials,
            profile.HasStatusViolations,
            profile.Notes,
            profile.CreatedAt,
            profile.UpdatedAt
        ));
    }

    /// <summary>
    /// Create user profile
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserProfileDto>> CreateUserProfile(CreateUserProfileDto dto)
    {
        // Check if profile already exists
        var existingProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == dto.UserId);

        if (existingProfile != null)
        {
            return BadRequest("User profile already exists.");
        }

        var profile = new UserProfile
        {
            UserId = dto.UserId,
            DateOfBirth = dto.DateOfBirth,
            CitizenshipCountryId = dto.CitizenshipCountryId,
            MaritalStatus = dto.MaritalStatus,
            HasRelativesInUS = dto.HasRelativesInUS,
            HasJobOffer = dto.HasJobOffer,
            EducationLevel = dto.EducationLevel,
            FearOfPersecution = dto.FearOfPersecution,
            HasPastVisaDenials = dto.HasPastVisaDenials,
            HasStatusViolations = dto.HasStatusViolations,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Reload with country information
        await _context.Entry(profile)
            .Reference(p => p.CitizenshipCountry)
            .LoadAsync();

        return CreatedAtAction(
            nameof(GetUserProfile),
            new { userId = profile.UserId },
            new UserProfileDto(
                profile.Id,
                profile.UserId,
                profile.DateOfBirth,
                profile.CitizenshipCountryId,
                profile.CitizenshipCountry?.Name,
                profile.MaritalStatus,
                profile.HasRelativesInUS,
                profile.HasJobOffer,
                profile.EducationLevel,
                profile.FearOfPersecution,
                profile.HasPastVisaDenials,
                profile.HasStatusViolations,
                profile.Notes,
                profile.CreatedAt,
                profile.UpdatedAt
            )
        );
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("user/{userId:guid}")]
    public async Task<ActionResult<UserProfileDto>> UpdateUserProfile(Guid userId, UpdateUserProfileDto dto)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        // Update fields
        profile.DateOfBirth = dto.DateOfBirth;
        profile.CitizenshipCountryId = dto.CitizenshipCountryId;
        profile.MaritalStatus = dto.MaritalStatus;
        profile.HasRelativesInUS = dto.HasRelativesInUS;
        profile.HasJobOffer = dto.HasJobOffer;
        profile.EducationLevel = dto.EducationLevel;
        profile.FearOfPersecution = dto.FearOfPersecution;
        profile.HasPastVisaDenials = dto.HasPastVisaDenials;
        profile.HasStatusViolations = dto.HasStatusViolations;
        profile.Notes = dto.Notes;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with country information
        await _context.Entry(profile)
            .Reference(p => p.CitizenshipCountry)
            .LoadAsync();

        return Ok(new UserProfileDto(
            profile.Id,
            profile.UserId,
            profile.DateOfBirth,
            profile.CitizenshipCountryId,
            profile.CitizenshipCountry?.Name,
            profile.MaritalStatus,
            profile.HasRelativesInUS,
            profile.HasJobOffer,
            profile.EducationLevel,
            profile.FearOfPersecution,
            profile.HasPastVisaDenials,
            profile.HasStatusViolations,
            profile.Notes,
            profile.CreatedAt,
            profile.UpdatedAt
        ));
    }

    /// <summary>
    /// Delete user profile
    /// </summary>
    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> DeleteUserProfile(Guid userId)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        _context.UserProfiles.Remove(profile);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}