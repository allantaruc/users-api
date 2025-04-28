namespace Users.Api.Models.Auth;

/// <summary>
/// JWT configuration settings from appsettings.json
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key used for signing the token
    /// </summary>
    public required string SecretKey { get; set; }
    
    /// <summary>
    /// Issuer of the JWT token (typically your application name or URL)
    /// </summary>
    public required string Issuer { get; set; }
    
    /// <summary>
    /// Audience of the JWT token (typically your application name or client URL)
    /// </summary>
    public required string Audience { get; set; }
    
    /// <summary>
    /// Expiration time of the token in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
} 