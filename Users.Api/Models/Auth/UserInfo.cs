namespace Users.Api.Models.Auth;

/// <summary>
/// Basic user information returned after authentication
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// User's email address
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// User's first name
    /// </summary>
    public required string FirstName { get; set; }
    
    /// <summary>
    /// User's last name
    /// </summary>
    public required string LastName { get; set; }
} 