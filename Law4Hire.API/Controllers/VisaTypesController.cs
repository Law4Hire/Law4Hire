using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisaTypesController(IVisaTypeRepository visaTypeRepository) : ControllerBase
{
    private readonly IVisaTypeRepository _visaTypeRepository = visaTypeRepository;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VisaType>>> GetVisaTypes()
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
