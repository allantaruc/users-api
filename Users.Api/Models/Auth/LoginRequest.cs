namespace Users.Api.Models.Auth;

/// <summary>
/// Model for login requests
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address for authentication
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// User's password for authentication
    /// </summary>
    public required string Password { get; set; }
}