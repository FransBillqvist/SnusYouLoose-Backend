using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Services;

public class UserService : IUserService
{
    private readonly IGenericMongoRepository<User> _userRepository;
    public UserService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<User> userRepository)
    {
        _userRepository = userRepository;
        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }
    public async Task<User> GetUserAsync(string id) => await _userRepository.FindOneAsync(x => x.Id == id);
    public async Task CreateUserAsync(User newUser) => await _userRepository.InsertOneAsync(newUser);
    public async Task UpdateUserAsync(User updatedUser) => await _userRepository.ReplaceOneAsync(updatedUser);
    public async Task RemoveUserAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _userRepository.DeleteOneAsync(x => x.Id == id);
    }
}