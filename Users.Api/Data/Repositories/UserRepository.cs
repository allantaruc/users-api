using Microsoft.EntityFrameworkCore;
using Users.Api.Models;

namespace Users.Api.Data.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User> CreateUserAsync(User user)
    {
        // Validate email uniqueness
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with email '{user.Email}' already exists.");
        }
        
        // Validate employment dates
        if (user.Employments.Count != 0)
        {
            foreach (var employment in user.Employments)
            {
                if (employment is { EndDate: not null, StartDate: not null } && 
                    employment.EndDate.Value <= employment.StartDate.Value)
                {
                    throw new ArgumentException(
                        $"Employment end date ({employment.EndDate}) must be after start date ({employment.StartDate}) for company '{employment.Company}'.");
                }
            }
        }

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await dbContext.Users
            .Include(u => u.Address)
            .Include(u => u.Employments)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
} 