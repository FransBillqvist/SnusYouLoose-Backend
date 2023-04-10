using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Services;

public class Service<T> : Object
{
    private readonly IMongoCollection<T> _serviceCollection;

    public Service(
        IOptions<SnuffDatabaseSettings> snuffDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                snuffDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                    snuffDatabaseSettings.Value.DatabaseName);

            _serviceCollection = mongoDatabase.GetCollection<T>(
                    snuffDatabaseSettings.Value.SnuffCollection);
        }
    
    public async Task<List<T>> GetAsync() => 
        await _serviceCollection.Find(_ => true).ToListAsync();

    // public async Task<T?> GetAsync(string id) =>
    //     await _serviceCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // public async Task CreateAsync(T newObject) =>
    //     await _serviceCollection.InsertOneAsync(newObject);

    // public async Task UpdateAsync(string id, T updatedObject) =>
    //     await _serviceCollection.ReplaceOneAsync(x => x.Id == id, updatedObject);

    // public async Task RemoveAsync(string id) =>
    //     await _serviceCollection.DeleteOneAsync(x => x.Id == id);
}