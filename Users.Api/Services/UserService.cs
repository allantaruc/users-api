using FluentValidation;
using Users.Api.Data.Repositories;
using Users.Api.Models;

namespace Users.Api.Services;

public class UserService(IUserRepository userRepository, IValidator<User> userValidator)
    : IUserService
{
    public async Task<User> CreateUserAsync(User request)
    {
        // Manually validate the request
        var validationResult = await userValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.First();
            throw new ArgumentException(error.ErrorMessage);
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Address = request.Address,
            Employments = request.Employments
        };

        // Save to repository
        return await userRepository.CreateUserAsync(user);
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        var user = await userRepository.GetUserByIdAsync(id) ?? 
                   throw new InvalidOperationException($"User with ID {id} not found.");
        return user;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllUsersAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        await userRepository.DeleteUserAsync(id);
    }

    public async Task<User> UpdateUserAsync(int id, User request)
    {
        // Get existing user
        var existingUser = await userRepository.GetUserByIdAsync(id);
        
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User with ID {id} not found.");
        }
        
        // Manually validate the request
        var validationResult = await userValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.First();
            throw new ArgumentException(error.ErrorMessage);
        }
        
        // Update user properties
        existingUser.FirstName = request.FirstName;
        existingUser.LastName = request.LastName;
        existingUser.Email = request.Email;
        
        // Update address if provided
        if (request.Address != null)
        {
            existingUser.Address = request.Address;
        }
        
        // Update employments if provided
        if (request.Employments.Count > 0)
        {
            existingUser.Employments = request.Employments;
        }
        
        // Save to repository
        return await userRepository.UpdateUserAsync(existingUser);
    }
} 