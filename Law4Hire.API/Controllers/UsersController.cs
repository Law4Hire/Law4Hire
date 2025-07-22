using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Applied globally to the controller, but can be overridden per-action
[EnableRateLimiting("fixed")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
            user.Email,
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
            user.Category
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
            user.Email,
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
            user.Category
        );

        return Ok(userDto);
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
