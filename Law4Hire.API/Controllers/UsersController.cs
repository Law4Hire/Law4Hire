using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.PreferredLanguage,
            user.CreatedAt,
            user.IsActive
        );

        return Ok(userDto);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            return BadRequest("A user with this email already exists.");
        }

        var user = new User
        {
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            PhoneNumber = createUserDto.PhoneNumber,
            PreferredLanguage = createUserDto.PreferredLanguage
        };

        var createdUser = await _userRepository.CreateAsync(user);

        var userDto = new UserDto(
            createdUser.Id,
            createdUser.Email,
            createdUser.FirstName,
            createdUser.LastName,
            createdUser.PhoneNumber,
            createdUser.PreferredLanguage,
            createdUser.CreatedAt,
            createdUser.IsActive
        );

        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
    }
}
