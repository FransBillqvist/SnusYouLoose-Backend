using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Services;

public class UserService
{
    private readonly IMongoCollection<User> _userCollection;

    public UserService(
        IOptions<SnuffDatabaseSettings> snuffDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                snuffDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                    snuffDatabaseSettings.Value.DatabaseName);

            _userCollection = mongoDatabase.GetCollection<User>(
                    snuffDatabaseSettings.Value.SnuffCollection);
        }
    
    public async Task<List<User>> GetAllUsersAsync() => 
        await _userCollection.Find(_ => true).ToListAsync();

    public async Task<User> GetUserAsync(string id) =>
        await _userCollection.Find(x => x.Uid == id).FirstOrDefaultAsync();

    public async Task CreateUserAsync(User newUser) =>
        await _userCollection.InsertOneAsync(newUser);

    public async Task UpdateUserAsync(string id, User updatedUser) =>
        await _userCollection.ReplaceOneAsync(x => x.Uid == id, updatedUser);

    public async Task RemoveUserAsync(string id) =>
        await _userCollection.DeleteOneAsync(x => x.Uid == id);
}