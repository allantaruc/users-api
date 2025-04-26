using Users.Api.Data.Repositories;
using Users.Api.Models;

namespace Users.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User> CreateUserAsync(User request)
    {
        // Validate request
        ValidateCreateUserRequest(request);
        
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Address = request.Address,
            Employments = request.Employments ?? []
        };

        // Save to repository
        return await userRepository.CreateUserAsync(user);
    }
    
    public async Task<User> UpdateUserAsync(int id, User request)
    {
        // Get existing user
        var existingUser = await userRepository.GetUserByIdAsync(id);
        
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User with ID {id} not found.");
        }
        
        // Validate request
        ValidateCreateUserRequest(request);
        
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
        if (request.Employments != null && request.Employments.Count > 0)
        {
            existingUser.Employments = request.Employments;
        }
        
        // Save to repository
        return await userRepository.UpdateUserAsync(existingUser);
    }

    private static void ValidateCreateUserRequest(User request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException("FirstName is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("LastName is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (request.Address != null)
        {
            if (string.IsNullOrWhiteSpace(request.Address.Street))
            {
                throw new ArgumentException("Street is required for Address.");
            }

            if (string.IsNullOrWhiteSpace(request.Address.City))
            {
                throw new ArgumentException("City is required for Address.");
            }
        }

        foreach (var employment in request.Employments)
        {
            if (string.IsNullOrWhiteSpace(employment.Company))
            {
                throw new ArgumentException("Company is required for Employment.");
            }

            if (employment.MonthsOfExperience == null)
            {
                throw new ArgumentException("MonthsOfExperience is required for Employment.");
            }

            if (employment.Salary == null)
            {
                throw new ArgumentException("Salary is required for Employment.");
            }

            if (employment.StartDate == null)
            {
                throw new ArgumentException("StartDate is required for Employment.");
            }
        }
    }
} 