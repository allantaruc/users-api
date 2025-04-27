using Microsoft.AspNetCore.Mvc;
using Users.Api.Models.Auth;
using Users.Api.Services;

namespace Users.Api.Controllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
/// <remarks>
/// Constructor with dependency injection
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Authentication response with JWT token if successful</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.RegisterAsync(request);
        if (result == null)
        {
            logger?.LogError("Register failed");
            return BadRequest("Registration failed. Email may already be in use or passwords don't match.");

        }
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with JWT token if successful</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.LoginAsync(request);
        if (result == null)
        {
            logger?.LogError("Login failed");
            return Unauthorized("Invalid email or password.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <returns>Status 200 if the token is valid</returns>
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateToken()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized("No valid authorization header found");

        string token = authHeader.Substring("Bearer ".Length).Trim();
        if (!authService.ValidateToken(token))
        {
            logger?.LogError("Invalid token");
            return Unauthorized("Invalid token");
        }

        return Ok(new { Valid = true });
    }
} 