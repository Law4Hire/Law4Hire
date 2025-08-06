using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserServicePackagesController : ControllerBase
{
    private readonly Law4HireDbContext _context;

    public UserServicePackagesController(Law4HireDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<IActionResult> CreateUserServicePackage([FromBody] CreateUserServicePackageRequest request)
    {
        try
        {
            var userServicePackage = new UserServicePackage
            {
                UserId = request.UserId,
                ServicePackageId = request.ServicePackageId,
                Paid = request.Paid,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserServicePackages.Add(userServicePackage);
            await _context.SaveChangesAsync();

            return Ok(new { Id = userServicePackage.Id, Message = "UserServicePackage created successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating UserServicePackage: {ex.Message}");
        }
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<IActionResult> GetUserServicePackage(int id)
    {
        try
        {
            var userServicePackage = await _context.UserServicePackages
                .Include(usp => usp.User)
                .Include(usp => usp.ServicePackage)
                .FirstOrDefaultAsync(usp => usp.Id == id);

            if (userServicePackage == null)
                return NotFound($"UserServicePackage with ID {id} not found.");

            return Ok(userServicePackage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving UserServicePackage: {ex.Message}");
        }
    }
}

public record CreateUserServicePackageRequest(
    Guid UserId,
    int ServicePackageId,
    bool Paid = false
);