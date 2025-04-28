using Users.Api.Models;

namespace Users.Api.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(User request);
    Task<User> UpdateUserAsync(int id, User request);
    Task<User> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task DeleteUserAsync(int id);
} 