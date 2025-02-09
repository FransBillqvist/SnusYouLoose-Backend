using DAL;
namespace Services.Interfaces;
public interface IUserService
{
    Task CreateUserAsync(User newUser);
    Task CreateUserV2Async(User newUser);
    Task<User> GetUserAsync(string id);
    Task RemoveUserAsync(string id);
    Task UpdateUserAsync(User updatedUser);
}