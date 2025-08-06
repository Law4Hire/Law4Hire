using BCrypt.Net;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using static Law4Hire.Application.Services.AuthService;

namespace Law4Hire.Application.Services;

public interface IAuthService
{
    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    Task<LoginResultWithRoute> LoginWithRouteAsync(LoginDto loginDto);
}

public interface ITokenService
{
    string CreateToken(Law4Hire.Core.Entities.User user);
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

    // NOTE: This method is disabled - authentication moved to Identity system
    /*
    public async Task<LoginResultWithRoute> LoginWithRouteAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(u => u.VisaInterview)
            .Include(u => u.Documents)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, Convert.ToBase64String(user.PasswordHash)))
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
    */

    // Stub implementation to satisfy interface
    public Task<LoginResultWithRoute> LoginWithRouteAsync(LoginDto loginDto)
    {
        throw new NotImplementedException("Authentication moved to ASP.NET Identity system. Use AuthController endpoints instead.");
    }
}