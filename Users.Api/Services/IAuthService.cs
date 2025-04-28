using Users.Api.Models.Auth;

namespace Users.Api.Services;

/// <summary>
/// Service for handling authentication and user management
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns token information
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with token if successful</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">Registration information</param>
    /// <returns>Authentication response with token if successful</returns>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>True if the token is valid</returns>
    bool ValidateToken(string token);
}