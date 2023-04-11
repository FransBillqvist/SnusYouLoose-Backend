using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Services;

public class SnuffService
{
    private readonly IMongoCollection<Snuff> _serviceCollection;

    public SnuffService(
        IOptions<SnuffDatabaseSettings> snuffDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                snuffDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                    snuffDatabaseSettings.Value.DatabaseName);

            _serviceCollection = mongoDatabase.GetCollection<Snuff>(
                    snuffDatabaseSettings.Value.SnuffCollection);
        }
    
    public async Task<List<Snuff>> GetAllSnuffAsync() => 
        await _serviceCollection.Find(_ => true).ToListAsync();

    public async Task<Snuff> GetSnuffAsync(string id) =>
        await _serviceCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateSnuffAsync(Snuff newSnuff) =>
        await _serviceCollection.InsertOneAsync(newSnuff);

    public async Task UpdateSnuffAsync(string id, Snuff updatedSnuff) =>
        await _serviceCollection.ReplaceOneAsync(x => x.Id == id, updatedSnuff);

    public async Task RemoveSnuffAsync(string id) =>
        await _serviceCollection.DeleteOneAsync(x => x.Id == id);
}