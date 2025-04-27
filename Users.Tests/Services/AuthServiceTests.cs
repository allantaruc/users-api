using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Users.Api.Data;
using Users.Api.Models;
using Users.Api.Models.Auth;
using Users.Api.Services;

namespace Users.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private AppDbContext _dbContext = null!;
    private AuthService _authService = null!;
    private const string TestDbName = "AuthServiceTests";

    [SetUp]
    public void Setup()
    {
        // Create a new in-memory database for each test
        _dbContext = TestHelpers.CreateInMemoryDbContext(TestDbName + Guid.NewGuid().ToString());
        _authService = TestHelpers.CreateAuthService(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task RegisterAsync_WhenValidDetails_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.Expiration.ShouldBeGreaterThan(0);
        result.User.ShouldNotBeNull();
        result.User.Email.ShouldBe(request.Email);
        result.User.FirstName.ShouldBe(request.FirstName);
        result.User.LastName.ShouldBe(request.LastName);

        // Verify token can be parsed
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        token.ShouldNotBeNull();

        // Verify user was created in the database
        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        userInDb.ShouldNotBeNull();
        userInDb.PasswordHash.ShouldNotBeNullOrEmpty();
        userInDb.PasswordSalt.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task RegisterAsync_WhenEmailExists_ShouldReturnNull()
    {
        // Arrange
        var existingUser = new User
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com"
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.doe@example.com", // Same email as existing user
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task RegisterAsync_WhenPasswordsDontMatch_ShouldReturnNull()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "DifferentPassword"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task LoginAsync_WhenValidCredentials_ShouldReturnToken()
    {
        // Arrange - Register a user
        var registerRequest = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.Expiration.ShouldBeGreaterThan(0);
        result.User.ShouldNotBeNull();
        result.User.Email.ShouldBe(loginRequest.Email);
    }

    [Test]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "P@ssw0rd123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task LoginAsync_WhenIncorrectPassword_ShouldReturnNull()
    {
        // Arrange - Register a user
        var registerRequest = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "john.doe@example.com",
            Password = "WrongPassword"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task ValidateToken_WhenValidToken_ShouldReturnTrue()
    {
        // Arrange - Register a user and get token
        var registerRequest = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };
        var authResponse = await _authService.RegisterAsync(registerRequest);
        var token = authResponse.Token;

        // Act
        var result = _authService.ValidateToken(token);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void ValidateToken_WhenInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        // Act
        var result = _authService.ValidateToken(invalidToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void ValidateToken_WhenEmptyToken_ShouldReturnFalse()
    {
        // Act
        var result = _authService.ValidateToken(string.Empty);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void ValidateToken_WhenNullToken_ShouldReturnFalse()
    {
        // Act
        var result = _authService.ValidateToken(null!);

        // Assert
        result.ShouldBeFalse();
    }
} 