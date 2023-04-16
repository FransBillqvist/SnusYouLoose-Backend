using DAL;
using DAL.Interfaces;
using MongoDB.Bson;

namespace Services;

public class UserService
{
    private readonly IGenericMongoRepository<User> _userRepository;
    // private readonly IServiceProvider _serviceProvider;
    public UserService(IGenericMongoRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // public async Task<List<User>> GetAllUsersAsync()
    // {
    //     var dummy = new DateTime(2022, 1, 1);
    //      var repsone =  await _userRepo.FilterBy(o => o.CreatedAtUtc > dummy);
    // }
    public async Task<User> GetUserAsync(string id) => await _userRepository.FindByIdAsync(id);
    public async Task CreateUserAsync(User newUser) => await _userRepository.InsertOneAsync(newUser);
    public async Task UpdateUserAsync(User updatedUser) => await _userRepository.ReplaceOneAsync(updatedUser);
    public async Task RemoveUserAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _userRepository.DeleteOneAsync(x => x.Id == mongoId.ToString());
    }
}