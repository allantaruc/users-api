using Users.Api.Models;

namespace Users.Api.Data.Repositories;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(int id);
} 