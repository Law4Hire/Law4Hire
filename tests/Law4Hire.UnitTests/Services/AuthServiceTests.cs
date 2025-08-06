using Law4Hire.Application.Services;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Law4Hire.UnitTests.Services;

[TestFixture]
public class AuthServiceTests
{
    private AuthService _authService = null!;
    private Mock<ITokenService> _mockTokenService = null!;
    private Law4HireDbContext _context = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<Law4HireDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new Law4HireDbContext(options);
        _mockTokenService = new Mock<ITokenService>();
        _authService = new AuthService(_context, _mockTokenService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void CreatePasswordHash_GeneratesValidHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        _authService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        // Assert
        Assert.That(passwordHash, Is.Not.Null, "Password hash should not be null");
        Assert.That(passwordSalt, Is.Not.Null, "Password salt should not be null");
        Assert.That(passwordHash.Length, Is.GreaterThan(0), "Password hash should not be empty");
        Assert.That(passwordSalt.Length, Is.GreaterThan(0), "Password salt should not be empty");
        Assert.That(passwordHash.Length, Is.EqualTo(64), "HMACSHA512 should produce 64-byte hash");
        Assert.That(passwordSalt.Length, Is.EqualTo(128), "HMACSHA512 key should be 128 bytes");
    }

    [Test]
    public void CreatePasswordHash_DifferentPasswords_GenerateDifferentHashes()
    {
        // Arrange
        var password1 = "Password1";
        var password2 = "Password2";

        // Act
        _authService.CreatePasswordHash(password1, out byte[] hash1, out byte[] salt1);
        _authService.CreatePasswordHash(password2, out byte[] hash2, out byte[] salt2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2), "Different passwords should generate different hashes");
        Assert.That(salt1, Is.Not.EqualTo(salt2), "Different passwords should generate different salts");
    }

    [Test]
    public void CreatePasswordHash_SamePassword_GeneratesDifferentSalts()
    {
        // Arrange
        var password = "SamePassword123";

        // Act
        _authService.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        _authService.CreatePasswordHash(password, out byte[] hash2, out byte[] salt2);

        // Assert
        Assert.That(salt1, Is.Not.EqualTo(salt2), "Same password should generate different salts each time");
        Assert.That(hash1, Is.Not.EqualTo(hash2), "Same password with different salts should generate different hashes");
    }

    [Test]
    public void VerifyPasswordHash_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        _authService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        // Act
        var result = _authService.VerifyPasswordHash(password, passwordHash, passwordSalt);

        // Assert
        Assert.That(result, Is.True, "Correct password should verify successfully");
    }

    [Test]
    public void VerifyPasswordHash_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var incorrectPassword = "WrongPassword123!";
        _authService.CreatePasswordHash(correctPassword, out byte[] passwordHash, out byte[] passwordSalt);

        // Act
        var result = _authService.VerifyPasswordHash(incorrectPassword, passwordHash, passwordSalt);

        // Assert
        Assert.That(result, Is.False, "Incorrect password should not verify");
    }

    [Test]
    public void VerifyPasswordHash_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        _authService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        // Act
        var result = _authService.VerifyPasswordHash("", passwordHash, passwordSalt);

        // Assert
        Assert.That(result, Is.False, "Empty password should not verify");
    }

    [Test]
    public void VerifyPasswordHash_NullPassword_ThrowsException()
    {
        // Arrange
        var password = "TestPassword123!";
        _authService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _authService.VerifyPasswordHash(null!, passwordHash, passwordSalt),
            "Null password should throw ArgumentNullException");
    }

    [Test]
    public void VerifyPasswordHash_InvalidHash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        _authService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
        
        // Corrupt the hash
        passwordHash[0] = (byte)(passwordHash[0] ^ 1);

        // Act
        var result = _authService.VerifyPasswordHash(password, passwordHash, passwordSalt);

        // Assert
        Assert.That(result, Is.False, "Corrupted hash should not verify");
    }

    [Test]
    public void VerifyPasswordHash_InvalidSalt_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        _authService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
        
        // Corrupt the salt
        passwordSalt[0] = (byte)(passwordSalt[0] ^ 1);

        // Act
        var result = _authService.VerifyPasswordHash(password, passwordHash, passwordSalt);

        // Assert
        Assert.That(result, Is.False, "Corrupted salt should not verify");
    }

    [Test]
    public void LoginWithRouteAsync_IsDisabled_ThrowsNotImplementedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "password"
        };

        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _authService.LoginWithRouteAsync(loginDto),
            "LoginWithRouteAsync should throw NotImplementedException as it's moved to Identity system");
    }

    [Test]
    public void CreatePasswordHash_SpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var passwordWithSpecialChars = "P@ssw0rd!#$%^&*()";

        // Act
        _authService.CreatePasswordHash(passwordWithSpecialChars, out byte[] passwordHash, out byte[] passwordSalt);
        var isValid = _authService.VerifyPasswordHash(passwordWithSpecialChars, passwordHash, passwordSalt);

        // Assert
        Assert.That(passwordHash, Is.Not.Null, "Should handle special characters in password");
        Assert.That(isValid, Is.True, "Should correctly verify password with special characters");
    }

    [Test]
    public void CreatePasswordHash_UnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var unicodePassword = "Pässwörd123ñ";

        // Act
        _authService.CreatePasswordHash(unicodePassword, out byte[] passwordHash, out byte[] passwordSalt);
        var isValid = _authService.VerifyPasswordHash(unicodePassword, passwordHash, passwordSalt);

        // Assert
        Assert.That(passwordHash, Is.Not.Null, "Should handle Unicode characters in password");
        Assert.That(isValid, Is.True, "Should correctly verify password with Unicode characters");
    }

    [Test]
    public void CreatePasswordHash_VeryLongPassword_HandlesCorrectly()
    {
        // Arrange
        var longPassword = new string('a', 1000); // 1000 character password

        // Act
        _authService.CreatePasswordHash(longPassword, out byte[] passwordHash, out byte[] passwordSalt);
        var isValid = _authService.VerifyPasswordHash(longPassword, passwordHash, passwordSalt);

        // Assert
        Assert.That(passwordHash, Is.Not.Null, "Should handle very long passwords");
        Assert.That(isValid, Is.True, "Should correctly verify very long password");
    }

    [Test]
    public void PasswordHash_ConsistentAcrossInstances()
    {
        // Arrange
        var password = "ConsistencyTest123!";
        var authService1 = new AuthService(_context, _mockTokenService.Object);
        var authService2 = new AuthService(_context, _mockTokenService.Object);

        // Act
        authService1.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        var verifyResult = authService2.VerifyPasswordHash(password, hash1, salt1);

        // Assert
        Assert.That(verifyResult, Is.True, "Password hash should be verifiable across different service instances");
    }
}