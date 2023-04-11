using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Services;

public class CurrentSnuffService
{
    private readonly IMongoCollection<CurrentSnuff> _serviceCollection;

    public CurrentSnuffService(
        IOptions<SnuffDatabaseSettings> snuffDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                snuffDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                    snuffDatabaseSettings.Value.DatabaseName);

            _serviceCollection = mongoDatabase.GetCollection<CurrentSnuff>(
                    snuffDatabaseSettings.Value.SnuffCollection);
        }
    
    public async Task<List<CurrentSnuff>> GetAllCurrentSnuffAsync() => 
        await _serviceCollection.Find(_ => true).ToListAsync();

    public async Task<CurrentSnuff> GetCurrentSnuffAsync(string id) =>
        await _serviceCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff) =>
        await _serviceCollection.InsertOneAsync(newCurrentSnuff);

    public async Task UpdateCurrentSnuffAsync(string id, CurrentSnuff updatedCurrentSnuff) =>
        await _serviceCollection.ReplaceOneAsync(x => x.Id == id, updatedCurrentSnuff);

    public async Task RemoveCurrentSnuffAsync(string id) =>
        await _serviceCollection.DeleteOneAsync(x => x.Id == id);
}