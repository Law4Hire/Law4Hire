using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class AdminController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IUserRepository _userRepository;
    private readonly IServicePackageRepository _servicePackageRepository;
    private readonly Law4HireDbContext _context;

    public AdminController(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IUserRepository userRepository,
        IServicePackageRepository servicePackageRepository,
        Law4HireDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _servicePackageRepository = servicePackageRepository;
        _context = context;
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    [HttpPost("users")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest($"User with email {request.Email} already exists.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = true,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                PreferredLanguage = "en",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Category = request.IsAdmin ? "Admin" : "User"
            };

            var result = await _userManager.CreateAsync(newUser, "SecureTest123!");
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Add to roles
            if (request.IsAdmin)
            {
                await _userManager.AddToRoleAsync(newUser, "Admin");
            }
            if (request.IsLegalProfessional)
            {
                await _userManager.AddToRoleAsync(newUser, "LegalProfessionals");
            }

            var userDto = new UserDto(
                newUser.Id,
                newUser.Email ?? "",
                newUser.FirstName,
                newUser.MiddleName,
                newUser.LastName,
                newUser.PhoneNumber,
                newUser.PreferredLanguage,
                newUser.CreatedAt,
                newUser.IsActive,
                newUser.Address1,
                newUser.Address2,
                newUser.City,
                newUser.State,
                newUser.Country,
                newUser.PostalCode,
                newUser.DateOfBirth,
                newUser.Category,
                newUser.CitizenshipCountryId,
                newUser.CitizenshipCountry?.Name,
                newUser.MaritalStatus
            );

            return CreatedAtAction(nameof(GetAllUsers), new { id = newUser.Id }, userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet("users")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        try
        {
            // Get all users from our custom User table
            var users = await _context.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            var userDtos = users.Select(u => new UserDto(
                u.Id,
                u.Email ?? "",
                u.FirstName,
                u.MiddleName,
                u.LastName,
                u.PhoneNumber,
                u.PreferredLanguage,
                u.CreatedAt,
                u.IsActive,
                u.Address1,
                u.Address2,
                u.City,
                u.State,
                u.Country,
                u.PostalCode,
                u.DateOfBirth,
                u.Category,
                u.CitizenshipCountryId,
                u.CitizenshipCountry?.Name,
                u.MaritalStatus
            )).ToList();

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving users: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggle admin role for a user
    /// </summary>
    [HttpPost("users/{userId:guid}/admin")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<IActionResult> ToggleAdminRole(Guid userId, [FromBody] ToggleAdminDto request)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
                return NotFound($"User with ID {userId} not found.");

            var isCurrentlyAdmin = await _userManager.IsInRoleAsync(identityUser, "Admin");

            if (request.IsAdmin && !isCurrentlyAdmin)
            {
                // Add to Admin role
                await _userManager.AddToRoleAsync(identityUser, "Admin");
            }
            else if (!request.IsAdmin && isCurrentlyAdmin)
            {
                // Remove from Admin role
                await _userManager.RemoveFromRoleAsync(identityUser, "Admin");
            }

            return Ok(new { Success = true, IsAdmin = request.IsAdmin });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating admin role: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset user password to default
    /// </summary>
    [HttpPost("users/{userId:guid}/resetPassword")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<IActionResult> ResetUserPassword(Guid userId)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
                return NotFound($"User with ID {userId} not found.");

            // Generate a new password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
            
            // Set new password to "SecureTest123!"
            var result = await _userManager.ResetPasswordAsync(identityUser, resetToken, "SecureTest123!");
            
            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "Password reset to 'SecureTest123!'" });
            }
            else
            {
                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error resetting password: {ex.Message}");
        }
    }

    /// <summary>
    /// Update service package pricing
    /// </summary>
    [HttpPut("servicePackages/{packageId:int}")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<IActionResult> UpdateServicePackage(int packageId, [FromBody] UpdateServicePackageDto request)
    {
        try
        {
            var package = await _servicePackageRepository.GetByIdAsync(packageId);
            if (package == null)
                return NotFound($"Service package with ID {packageId} not found.");

            // Update package properties
            package.Name = request.Name;
            package.Description = request.Description;
            package.Type = request.Type;
            package.BasePrice = request.BasePrice;
            package.L4HLLCFee = request.L4HLLCFee;
            package.HasMoneyBackGuarantee = request.HasMoneyBackGuarantee;
            package.IsActive = request.IsActive;
            package.LastModifiedAt = DateTime.UtcNow;

            await _servicePackageRepository.UpdateAsync(package);

            return Ok(new { Success = true, Message = "Service package updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating service package: {ex.Message}");
        }
    }

    /// <summary>
    /// Get service packages grouped by visa type
    /// </summary>
    [HttpGet("servicePackages")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<ActionResult<List<VisaTypePackageDto>>> GetAllServicePackages([FromQuery] bool includeDeprecated = false)
    {
        try
        {
            // Get all BaseVisaTypes based on whether to include deprecated
            var baseVisaTypesQuery = _context.BaseVisaTypes.AsQueryable();
            if (!includeDeprecated)
            {
                baseVisaTypesQuery = baseVisaTypesQuery.Where(bvt => bvt.Status == "Active");
            }
            var allBaseVisaTypes = await baseVisaTypesQuery.OrderBy(bvt => bvt.Code).ToListAsync();

            // Handle duplicates - group by VisaName and select first occurrence
            var uniqueBaseVisaTypes = allBaseVisaTypes
                .GroupBy(bvt => bvt.Code)
                .Select(g => g.First()) // Select first occurrence of each name
                .ToList();

            // Get all service packages and include VisaType for name matching
            var packages = await _context.ServicePackages
                .Include(sp => sp.VisaType)
                .Where(sp => sp.IsActive)
                .ToListAsync();

            // Group packages by VisaType name (not ID) and create grouped data
            var groupedData = uniqueBaseVisaTypes.Select(baseVisaType =>
            {
                // Find packages that match this BaseVisaType by name
                var visaPackages = packages
                    .Where(p => p.VisaType != null && p.VisaType.Code == baseVisaType.Code)
                    .Select(p => new ServicePackageDto(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Type,
                        p.BasePrice,
                        p.L4HLLCFee,
                        p.HasMoneyBackGuarantee,
                        p.IsActive,
                        p.VisaTypeId,
                        p.VisaType?.Code
                    )).ToList();

                return new VisaTypePackageDto(
                    baseVisaType.Id,
                    baseVisaType.Code,
                    baseVisaType.Description, // Use VisaDescription as description
                    baseVisaType.Status,
                    visaPackages,
                    visaPackages.FirstOrDefault() // Select first package as default
                );
            })
            .Where(vtpd => vtpd.Packages.Any() || includeDeprecated) // Only include if has packages or showing deprecated
            .ToList();

            return Ok(groupedData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving service packages: {ex.Message}");
        }
    }

    /// <summary>
    /// Update BaseVisaType status (Active/Deprecated)
    /// </summary>
    [HttpPut("visaTypes/{visaTypeId:guid}/status")]
    [AllowAnonymous] // TEMPORARY: Remove when authentication is fully implemented
    public async Task<IActionResult> UpdateVisaTypeStatus(Guid visaTypeId, [FromBody] UpdateVisaTypeStatusDto request)
    {
        try
        {
            var baseVisaType = await _context.BaseVisaTypes.FirstOrDefaultAsync(bvt => bvt.Id == visaTypeId);
            if (baseVisaType == null)
                return NotFound($"VisaType with ID {visaTypeId} not found.");

            baseVisaType.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = $"VisaType status updated to {request.Status}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating visa type status: {ex.Message}");
        }
    }
}

public record ToggleAdminDto(bool IsAdmin);
public record UpdateVisaTypeStatusDto(string Status);
public record CreateUserDto(string Email, string FirstName, string LastName, string PhoneNumber, bool IsAdmin, bool IsLegalProfessional);