using Microsoft.EntityFrameworkCore;
using Users.Api.Models;

namespace Users.Api.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _dbContext.Users
            .Include(u => u.Address)
            .Include(u => u.Employments)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
} 