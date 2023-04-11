using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
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
    
    public async Task<List<SnuffLog>> GetAllSnuffLogsAsync() => 
        await _serviceCollection.Find(_ => true).ToListAsync();

    public async Task<SnuffLog> GetSnuffLogAsync(string id) =>
        await _serviceCollection.Find(x => x.SnuffLogId == id).FirstOrDefaultAsync();

    public async Task CreateSnuffLogAsync(SnuffLog newSnuffLog) =>
        await _serviceCollection.InsertOneAsync(newSnuffLog);

    public async Task UpdateSnuffLogAsync(string id, SnuffLog updatedSnuffLog) =>
        await _serviceCollection.ReplaceOneAsync(x => x.SnuffLogId == id, updatedSnuffLog);

    public async Task RemoveSnuffLogAsync(string id) =>
        await _serviceCollection.DeleteOneAsync(x => x.SnuffLogId == id);
}