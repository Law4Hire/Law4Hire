// DEPRECATED: This controller is no longer used. 
// Legal professional functionality has been moved to the AdminController with role-based access.
// Users with LegalProfessionals role are now managed through the AspNetRoles system.

/*
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LegalProfessionalController(Law4HireDbContext context) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(LegalProfessionalCreateDto dto)
    {
        var exists = await context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists) return BadRequest("User already exists.");

        var user = new User
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            PreferredLanguage = dto.PreferredLanguage ?? "en",
            // TODO: Update to use Identity UserManager
            // PasswordHash and PasswordSalt moved to Identity system
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.LegalProfessionals.Add(new LegalProfessional
        {
            Id = user.Id,
            BarNumber = dto.BarNumber ?? string.Empty,
            BarState = dto.BarState ?? string.Empty
        });

        await context.SaveChangesAsync();

        return Ok(new { Message = "Legal professional registered.", UserId = user.Id });
    }
}
*/
