using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Applied globally to the controller, but can be overridden per-action
[EnableRateLimiting("fixed")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly Law4HireDbContext _context;

    public UsersController(IUserRepository userRepository, UserManager<User> userManager, Law4HireDbContext context)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous] // <-- TEMPORARY: Allows testing without JWT
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var userDto = new UserDto(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.PhoneNumber,
            user.PreferredLanguage,
            user.CreatedAt,
            user.IsActive,
            user.Address1,
            user.Address2,
            user.City,
            user.State,
            user.Country,
            user.PostalCode,
            user.DateOfBirth,
            user.Category,
            user.CitizenshipCountryId,
            user.CitizenshipCountry?.Name,
            user.MaritalStatus
        );

        return Ok(userDto);
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous] // TEMPORARY: allow editing without auth
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        user.Email = updateDto.Email;
        user.FirstName = updateDto.FirstName;
        user.MiddleName = updateDto.MiddleName;
        user.LastName = updateDto.LastName;
        user.PhoneNumber = updateDto.PhoneNumber;
        user.PreferredLanguage = updateDto.PreferredLanguage;
        user.Address1 = updateDto.Address1;
        user.Address2 = updateDto.Address2;
        user.City = updateDto.City;
        user.State = updateDto.State;
        user.Country = updateDto.Country;
        user.PostalCode = updateDto.PostalCode;
        user.DateOfBirth = updateDto.DateOfBirth;

        await _userRepository.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.PhoneNumber,
            user.PreferredLanguage,
            user.CreatedAt,
            user.IsActive,
            user.Address1,
            user.Address2,
            user.City,
            user.State,
            user.Country,
            user.PostalCode,
            user.DateOfBirth,
            user.Category,
            user.CitizenshipCountryId,
            user.CitizenshipCountry?.Name,
            user.MaritalStatus
        );

        return Ok(userDto);
    }

    [HttpPut("{id:guid}/category")]
    [AllowAnonymous] // TEMPORARY: allow editing without auth
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserCategory(Guid id, [FromBody] UpdateCategoryDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        user.Category = updateDto.Category;
        await _userRepository.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.PhoneNumber,
            user.PreferredLanguage,
            user.CreatedAt,
            user.IsActive,
            user.Address1,
            user.Address2,
            user.City,
            user.State,
            user.Country,
            user.PostalCode,
            user.DateOfBirth,
            user.Category,
            user.CitizenshipCountryId,
            user.CitizenshipCountry?.Name,
            user.MaritalStatus
        );

        return Ok(userDto);
    }

    [HttpGet("{id:guid}/isAdmin")]
    [AllowAnonymous] // TEMPORARY: allow checking admin role without auth
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> IsAdmin(Guid id)
    {
        // Find the Identity user by their ID (stored as string in Identity)
        var identityUser = await _userManager.FindByIdAsync(id.ToString());
        if (identityUser == null)
            return NotFound($"User with ID {id} not found.");

        // Check if user is in Admin role
        var isAdmin = await _userManager.IsInRoleAsync(identityUser, "Admin");
        return Ok(isAdmin);
    }

    /// <summary>
    /// Get user's complete profile including service package and visa type information
    /// </summary>
    [HttpGet("{userId:guid}/profile")]
    [AllowAnonymous] // TEMPORARY: allow access without auth for testing
    [ProducesResponseType<UserProfileSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileSummaryDto>> GetUserProfile(Guid userId)
    {
        // Validate user exists - only select essential fields to avoid missing column errors
        var user = await _context.Users
            .Select(u => new { u.Id, u.UserName, u.Email })
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
            return NotFound($"User with ID {userId} not found.");

        // Get the user's most recent service package (if any) - handle missing tables
        UserServicePackage? userServicePackage = null;
        try
        {
            userServicePackage = await _context.UserServicePackages
                .Include(usp => usp.ServicePackage)
                    .ThenInclude(sp => sp.VisaType)
                        .ThenInclude(vt => vt.CategoryVisaTypes)
                            .ThenInclude(cvt => cvt.Category)
                .Where(usp => usp.UserId == userId)
                .OrderByDescending(usp => usp.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception)
        {
            // Table may not exist, continue with null
            userServicePackage = null;
        }

        // Map to DTOs
        SelectedPackageDto? selectedPackage = null;
        AssignedVisaTypeDto? assignedVisaType = null;

        if (userServicePackage?.ServicePackage != null)
        {
            var servicePackage = userServicePackage.ServicePackage;
            selectedPackage = new SelectedPackageDto(
                servicePackage.Id,
                servicePackage.Name,
                servicePackage.Description
            );

            if (servicePackage.VisaType != null)
            {
                var visaType = servicePackage.VisaType;
                var category = visaType.CategoryVisaTypes?.FirstOrDefault()?.Category?.Name ?? "Unknown";
                
                assignedVisaType = new AssignedVisaTypeDto(
                    visaType.Id,
                    visaType.Code,
                    category
                );
            }
        }

        var userProfile = new UserProfileSummaryDto(
            user.Id,
            user.UserName ?? user.Email ?? "Unknown",
            user.Email ?? "Unknown",
            selectedPackage,
            assignedVisaType
        );

        return Ok(userProfile);
    }

    //[HttpPost]
    //[ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[AllowAnonymous]
    //public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    //{
    //    var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
    //    if (existingUser != null)
    //    {
    //        return BadRequest("A user with this email already exists.");
    //    }

    //    var user = new User
    //    {
    //        Email = createUserDto.Email,
    //        FirstName = createUserDto.FirstName,
    //        LastName = createUserDto.LastName,
    //        PhoneNumber = createUserDto.PhoneNumber,
    //        PreferredLanguage = createUserDto.PreferredLanguage,
    //        Address1 = createUserDto.Address1,
    //        Address2 = createUserDto.Address2,
    //        City = createUserDto.City,
    //        State = createUserDto.State,
    //        Country = createUserDto.Country,
    //        PostalCode = createUserDto.PostalCode,
    //        DateOfBirth = createUserDto.DateOfBirth
    //    };

    //    var createdUser = await _userRepository.CreateAsync(user);

    //    var userDto = new UserDto(
    //        createdUser.Id,
    //        createdUser.Email,
    //        createdUser.FirstName,
    //        createdUser.MiddleName,
    //        createdUser.LastName,
    //        createdUser.PhoneNumber,
    //        createdUser.PreferredLanguage,
    //        createdUser.CreatedAt,
    //        createdUser.IsActive,
    //        createdUser.Address1,
    //        createdUser.Address2,
    //        createdUser.City,
    //        createdUser.State,
    //        createdUser.Country,
    //        createdUser.PostalCode,
    //        createdUser.DateOfBirth
    //    );

    //    return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
    //}
}
