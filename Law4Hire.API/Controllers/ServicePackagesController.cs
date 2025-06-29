using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data;
using Law4Hire.Infrastructure.Data.Repositories;

namespace Law4Hire.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class ServicePackagesController(ServicePackageRepository servicePackageRepository) : ControllerBase
{
    private readonly ServicePackageRepository _servicePackageRepository = servicePackageRepository;

    /// <summary>
    /// Get all active service packages
    /// </summary>
    /// <returns>List of active service packages</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ServicePackageDto>>(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServicePackageDto>>> GetActivePackages()
    {
        var packages = await _servicePackageRepository.GetAllActiveAsync();
        
        var packageDtos = packages.Select(p => new ServicePackageDto(
            p.Id,
            p.Name,
            p.Description,
            p.Type,
            p.BasePrice,
            p.HasMoneyBackGuarantee,
            p.IsActive
        ));

        return Ok(packageDtos);
    }

    /// <summary>
    /// Get service package by ID
    /// </summary>
    /// <param name="id">Service Package ID</param>
    /// <returns>Service package details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ServicePackageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<ServicePackageDto>> GetPackage(int id)
    {
        var package = await _servicePackageRepository.GetByIdAsync(id);
        if (package == null)
            return NotFound($"Service package with ID {id} not found.");

        var packageDto = new ServicePackageDto(
            package.Id,
            package.Name,
            package.Description,
            package.Type,
            package.BasePrice,
            package.HasMoneyBackGuarantee,
            package.IsActive
        );

        return Ok(packageDto);
    }
}
