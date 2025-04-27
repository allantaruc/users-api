namespace Users.Api.Models.Auth;

/// <summary>
/// Model for user registration
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// First name of the new user
    /// </summary>
    public required string FirstName { get; set; }
    
    /// <summary>
    /// Last name of the new user
    /// </summary>
    public required string LastName { get; set; }
    
    /// <summary>
    /// Email address for the new user (must be unique)
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Password for the new user
    /// </summary>
    public required string Password { get; set; }
    
    /// <summary>
    /// Password confirmation to ensure correct entry
    /// </summary>
    public required string ConfirmPassword { get; set; }
}