using DAL;

public interface IUserService
{
    Task CreateUserAsync(User newUser);
    Task<User> GetUserAsync(string id);
    Task RemoveUserAsync(string id);
    Task UpdateUserAsync(User updatedUser);
}