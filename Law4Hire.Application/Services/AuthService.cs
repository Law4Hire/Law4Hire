using Law4Hire.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Law4Hire.Core.DTOs;
using BCrypt.Net;
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.Application.Services;


public interface IAuthService
{
    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
}

public class AuthService : IAuthService
{
    private readonly Law4HireDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(Law4HireDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }


    public class LoginResultWithRoute
    {
        public string Token { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
    }


    public async Task<LoginResultWithRoute> LoginWithRouteAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(u => u.VisaInterview)
            .Include(u => u.Documents)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _tokenService.CreateToken(user);

        string route;
        if (user.VisaInterview?.IsReset == true)
            route = $"/interview?category={user.Category}";
        else if (user.Documents != null && user.Documents.Any())
            route = "/dashboard";
        else if (user.VisaInterview != null)
            route = "/interview/resume";
        else
            route = $"/interview?category={user.Category}";

        return new LoginResultWithRoute { Token = token, Route = route };
    }
}