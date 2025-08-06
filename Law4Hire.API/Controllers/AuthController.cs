using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly Law4HireDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(
        IUserRepository userRepository, 
        IAuthService authService,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        Law4HireDbContext context,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _authService = authService;
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto request)
    {
        if (await _userRepository.GetByEmailAsync(request.Email) != null)
        {
            return BadRequest("User with this email already exists.");
        }

        // Create user using Identity UserManager
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Address1 = request.Address1,
            Address2 = request.Address2,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            DateOfBirth = request.DateOfBirth,
            CitizenshipCountryId = request.CitizenshipCountryId,
            MaritalStatus = request.MaritalStatus,
            PreferredLanguage = "en", 
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Create UserProfile with extended information
        var userProfile = new UserProfile
        {
            UserId = user.Id,
            DateOfBirth = request.DateOfBirth,
            CitizenshipCountryId = request.CitizenshipCountryId,
            MaritalStatus = request.MaritalStatus,
            HasRelativesInUS = request.HasRelativesInUS,
            HasJobOffer = request.HasJobOffer,
            EducationLevel = request.EducationLevel,
            FearOfPersecution = request.FearOfPersecution,
            HasPastVisaDenials = request.HasPastVisaDenials,
            HasStatusViolations = request.HasStatusViolations,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        return Ok(new { UserId = user.Id, Message = "User registered successfully." });
    }

    //[HttpPost("login")]
    //public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    //{
    //    try
    //    {
    //        var result = await _authService.LoginWithRouteAsync(dto);
    //        return Ok(result);
    //    }
    //    catch (UnauthorizedAccessException)
    //    {
    //        return Unauthorized("Invalid credentials.");
    //    }
    //}
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        // Find user by email using Identity
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Check password using Identity's password verification
        var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordCheck)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Generate JWT token
        var token = _tokenService.CreateToken(user);
        
        return Ok(new { UserId = user.Id, Message = "Login successful.", Token = token });
    }

    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound();

        return Ok(new { Message = "Email exists." });
    }

    [HttpPost("reset-password-identity")]
    public async Task<IActionResult> ResetPasswordToIdentity([FromBody] ResetPasswordDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return NotFound($"User with email {request.Email} not found.");
        }

        // Remove existing password and set new one using Identity
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            return BadRequest($"Failed to remove existing password: {string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))}");
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            return BadRequest($"Failed to set new password: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");
        }

        return Ok(new { Message = $"Password reset successfully for {request.Email}" });
    }

    [HttpGet("debug-user/{email}")]
    public async Task<IActionResult> DebugUser(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"User with email {email} not found.");
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            UserNameLength = user.UserName?.Length ?? 0,
            user.NormalizedEmail,
            user.NormalizedUserName,
            user.EmailConfirmed,
            user.LockoutEnabled
        });
    }
}

public record ResetPasswordDto(string Email, string NewPassword);