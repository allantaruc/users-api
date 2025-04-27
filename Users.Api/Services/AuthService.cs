using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Users.Api.Data;
using Users.Api.Data.Repositories;
using Users.Api.Models;
using Users.Api.Models.Auth;

namespace Users.Api.Services;

/// <summary>
/// Service that handles user authentication and JWT token generation
/// </summary>
/// <remarks>
/// Constructor with dependency injection
/// </remarks>
public class AuthService(
    IUserRepository userRepository,
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthService> logger,
    AppDbContext dbContext) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user?.PasswordHash == null || user.PasswordSalt == null)
        {
            logger.LogWarning("Login failed: User not found or missing credentials: {Email}", request.Email);
            return null!;
        }

        if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            logger.LogWarning("Login failed: Invalid password for: {Email}", request.Email);
            return null!;
        }

        var token = GenerateJwtToken(user);
        var expiration = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes).ToUnixTimeSeconds();

        return new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!
            }
        };
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration failed: Email already exists: {Email}", request.Email);
            return null!;
        }

        if (request.Password != request.ConfirmPassword)
        {
            logger.LogWarning("Registration failed: Passwords don't match for: {Email}", request.Email);
            return null!;
        }

        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        var createdUser = await userRepository.CreateUserAsync(user);
        logger.LogInformation("User registered successfully: {Email}", request.Email);

        var token = GenerateJwtToken(createdUser);
        var expiration = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes).ToUnixTimeSeconds();

        return new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            User = new UserInfo
            {
                Id = createdUser.Id,
                Email = createdUser.Email!,
                FirstName = createdUser.FirstName!,
                LastName = createdUser.LastName!
            }
        };
    }

    /// <inheritdoc />
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName!),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Creates a password hash and salt
    /// </summary>
    private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        using var hmac = new HMACSHA512();
        var saltBytes = hmac.Key;
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        passwordSalt = Convert.ToBase64String(saltBytes);
        passwordHash = Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against the stored hash and salt
    /// </summary>
    private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var storedHashBytes = Convert.FromBase64String(storedHash);
        
        using var hmac = new HMACSHA512(saltBytes);
        var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        return computedHashBytes.SequenceEqual(storedHashBytes);
    }
}