using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisaTypesController(IVisaTypeRepository visaTypeRepository, Law4HireDbContext context) : ControllerBase
{
    private readonly IVisaTypeRepository _visaTypeRepository = visaTypeRepository;
    private readonly Law4HireDbContext _context = context;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BaseVisaType>>> GetVisaTypes()
    {
        var visaTypes = await _context.BaseVisaTypes
            .Where(vt => vt.Status == "Active")
            .OrderBy(vt => vt.Code)
            .ToListAsync();
        return Ok(visaTypes);
    }

    [HttpGet("legacy")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VisaType>>> GetLegacyVisaTypes()
    {
        var visaTypes = await _visaTypeRepository.GetAllAsync();
        return Ok(visaTypes);
    }

    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VisaType>>> GetByCategory(string category)
    {
        var visaTypes = await _visaTypeRepository.GetByCategoryAsync(category);
        return Ok(visaTypes);
    }
}
