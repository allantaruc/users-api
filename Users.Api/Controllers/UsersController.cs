using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Api.Models;
using Users.Api.Services;

namespace Users.Api.Controllers;

/// <summary>
/// Controller for managing user operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints in this controller
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">User details</param>
    /// <returns>The created user</returns>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User request)
    {
        try
        {
            var user = await userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user.");
        }
    }

    /// <summary>
    /// Gets all users - publicly accessible endpoint
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet]
    [AllowAnonymous] // Make this endpoint publicly accessible without authentication
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        try
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, "An error occurred while retrieving users.");
        }
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user details</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        try
        {
            var user = await userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"User with ID {id} not found.");
        }
    }
    
    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="user">Updated user details</param>
    /// <returns>The updated user</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> UpdateUser(int id, User user)
    {
        try
        {
            var updatedUser = await userService.UpdateUserAsync(id, user);
            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user");
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user.");
        }
    }
} 