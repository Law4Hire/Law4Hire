using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserRepository userRepository, IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto request)
    {
        if (await userRepository.GetByEmailAsync(request.Email) != null)
        {
            return BadRequest("User with this email already exists.");
        }

        authService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        var createdUser = await userRepository.CreateAsync(user);

        return Ok(new { UserId = createdUser.Id, Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        if (!authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized("Invalid credentials.");
        }

        // In a real app, you would generate and return a JWT token here.
        // For now, we'll return the user ID to signify success.
        return Ok(new { UserId = user.Id, Message = "Login successful." });
    }
}