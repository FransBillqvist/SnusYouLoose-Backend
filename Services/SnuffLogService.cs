using DAL;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Services;

public class SnuffLogService
{
    private readonly IMongoCollection<SnuffLog> _serviceCollection;

    public SnuffLogService(
        IOptions<SnuffDatabaseSettings> snuffDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                snuffDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                    snuffDatabaseSettings.Value.DatabaseName);

            _serviceCollection = mongoDatabase.GetCollection<SnuffLog>(
                    snuffDatabaseSettings.Value.SnuffCollection);
        }
    
    // public async Task<List<SnuffLog>> GetAllSnuffLogsAsync() => 
    //     await _serviceCollection.Find(_ => true).ToListAsync();

    // public async Task<SnuffLog> GetSnuffLogAsync(ObjectId id) =>
    //     await _serviceCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // public async Task CreateSnuffLogAsync(SnuffLog newSnuffLog) =>
    //     await _serviceCollection.InsertOneAsync(newSnuffLog);

    // public async Task UpdateSnuffLogAsync(ObjectId id, SnuffLog updatedSnuffLog) =>
    //     await _serviceCollection.ReplaceOneAsync(x => x.Id == id, updatedSnuffLog);

    // public async Task RemoveSnuffLogAsync(ObjectId id) =>
    //     await _serviceCollection.DeleteOneAsync(x => x.Id == id);
}