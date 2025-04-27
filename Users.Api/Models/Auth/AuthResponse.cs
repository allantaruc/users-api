namespace Users.Api.Models.Auth;

/// <summary>
/// Response model containing authentication token and user info
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT token for authentication
    /// </summary>
    public required string Token { get; set; }
    
    /// <summary>
    /// Expiration time of the token in Unix timestamp
    /// </summary>
    public long Expiration { get; set; }
    
    /// <summary>
    /// Basic user information
    /// </summary>
    public UserInfo? User { get; set; }
}