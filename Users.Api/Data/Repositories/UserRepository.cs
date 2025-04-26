using Microsoft.EntityFrameworkCore;
using Users.Api.Models;

namespace Users.Api.Data.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User> CreateUserAsync(User user)
    {
        await ValidateEmailUniquenessAsync(user);
        
        ValidateEmploymentDates(user.Employments);

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return (await dbContext.Users
            .Include(u => u.Address)
            .Include(u => u.Employments)
            .FirstOrDefaultAsync(u => u.Id == id) ?? null) ?? throw new InvalidOperationException();
    }
    
    public async Task<User> UpdateUserAsync(User user)
    {
        // Validate email uniqueness (excluding current user)
        await ValidateEmailUniquenessAsync(user, isUpdate: true);
        
        // Validate employment dates
        ValidateEmploymentDates(user.Employments);

        dbContext.Entry(user).State = EntityState.Modified;
        await dbContext.SaveChangesAsync();
        
        return user;
    }
    
    private async Task ValidateEmailUniquenessAsync(User user, bool isUpdate = false)
    {
        var query = dbContext.Users.Where(u => u.Email == user.Email);
        
        // When updating, exclude the current user from the uniqueness check
        if (isUpdate)
        {
            query = query.Where(u => u.Id != user.Id);
        }
        
        var existingUser = await query.FirstOrDefaultAsync();
        
        if (existingUser != null)
        {
            throw new InvalidOperationException(
                isUpdate
                    ? $"Another user with email '{user.Email}' already exists."
                    : $"A user with email '{user.Email}' already exists.");
        }
    }
    
    private static void ValidateEmploymentDates(List<Employment> employments)
    {
        if (employments.Count == 0)
        {
            return;
        }
        
        foreach (var employment in employments)
        {
            if (employment is { EndDate: not null, StartDate: not null } && 
                employment.EndDate.Value <= employment.StartDate.Value)
            {
                throw new ArgumentException(
                    $"Employment end date ({employment.EndDate}) must be after start date ({employment.StartDate}) for company '{employment.Company}'.");
            }
        }
    }
} 