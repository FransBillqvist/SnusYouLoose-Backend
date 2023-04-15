using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
    
    // public async Task<List<User>> GetAllUsersAsync() => 
    //     await _userCollection.Find(_ => true).ToListAsync();

    // public async Task<User> GetUserAsync(ObjectId id) =>
    //     await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // public async Task CreateUserAsync(User newUser) =>
    //     await _userCollection.InsertOneAsync(newUser);

    // public async Task UpdateUserAsync(ObjectId id, User updatedUser) =>
    //     await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    // public async Task RemoveUserAsync(ObjectId id) =>
    //     await _userCollection.DeleteOneAsync(x => x.Id == id);
}