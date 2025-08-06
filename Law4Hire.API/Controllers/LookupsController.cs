using Law4Hire.Core.DTOs;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly Law4HireDbContext _context;

    public LookupsController(Law4HireDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all UN-recognized countries in alphabetical order
    /// </summary>
    [HttpGet("countries")]
    public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountries()
    {
        var countries = await _context.Countries
            .Where(c => c.IsUNRecognized)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CountryDto(
                c.Id,
                c.Name,
                c.CountryCode,
                c.CountryCode2,
                c.IsUNRecognized,
                c.SortOrder
            ))
            .ToListAsync();

        return Ok(countries);
    }

    /// <summary>
    /// Get US states and territories
    /// </summary>
    [HttpGet("us-states")]
    public async Task<ActionResult<IEnumerable<USStateDto>>> GetUSStates()
    {
        var states = await _context.USStates
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new USStateDto(
                s.Id,
                s.Name,
                s.StateCode,
                s.Name, // StateName - use Name field
                !s.IsState, // IsTerritory is opposite of IsState
                s.SortOrder
            ))
            .ToListAsync();

        return Ok(states);
    }

    /// <summary>
    /// Get all visa categories
    /// </summary>
    [HttpGet("visa-categories")]
    public async Task<ActionResult<IEnumerable<VisaCategoryDto>>> GetVisaCategories()
    {
        var categories = await _context.VisaCategories
            .OrderBy(c => c.Name)
            .Select(c => new VisaCategoryDto(
                c.Id,
                c.Name,
                "", // Description - not in current entity
                "", // IconName - not in current entity
                true // IsActive - default to true
            ))
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get education levels
    /// </summary>
    [HttpGet("education-levels")]
    public ActionResult<IEnumerable<string>> GetEducationLevels()
    {
        var educationLevels = new[]
        {
            "Less than High School",
            "High School",
            "Some College",
            "Associate's Degree",
            "Bachelor's Degree",
            "Master's Degree",
            "Doctorate",
            "Professional Degree"
        };

        return Ok(educationLevels);
    }

    /// <summary>
    /// Get marital status options
    /// </summary>
    [HttpGet("marital-status")]
    public ActionResult<IEnumerable<string>> GetMaritalStatus()
    {
        var maritalStatusOptions = new[]
        {
            "Single",
            "Married",
            "Divorced",
            "Widowed",
            "Separated"
        };

        return Ok(maritalStatusOptions);
    }
}