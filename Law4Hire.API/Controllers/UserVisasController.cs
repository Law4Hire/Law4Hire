using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserVisasController(Law4HireDbContext context) : ControllerBase
{
    private readonly Law4HireDbContext _context = context;

    [HttpGet("user/{userId:guid}/exists")]
    public async Task<ActionResult<bool>> UserHasVisa(Guid userId)
    {
        var exists = await _context.UserVisas.AnyAsync(u => u.UserId == userId);
        return Ok(exists);
    }
}
