using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Services;

public class SnuffLogService
{
    private readonly IGenericMongoRepository<SnuffLog> _snuffLogRepository;

    public SnuffLogService(
        IOptions<MongoDbSettings> Settings)
    {
        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }
    public async Task CreateSnuffLogAsync(SnuffLog newSnuffLog) =>
        await _snuffLogRepository.InsertOneAsync(newSnuffLog);
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