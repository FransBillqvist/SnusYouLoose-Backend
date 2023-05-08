using DAL;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class SnuffService : ISnuffService
{
    private readonly IGenericMongoRepository<Snuff> _snuffRespository;

    public SnuffService(
        IOptions<MongoDbSettings> Settings)
    {
        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }

    // public async Task<List<Snuff>> GetAllSnuffAsync() => 
    //     await _snuffRespository.Find(_ => true).ToListAsync();

    public async Task<int> GetSnuffAmountAsync(string snuffId)
    {
        var response = await _snuffRespository.FindByIdAsync(snuffId);
        return response.DefaultAmount;
    }

    // public async Task<Snuff> GetSnuffAsync(ObjectId id) =>
    //     await _serviceCollection.FindAsync(x => x.id == Id).FirstOrDefaultAsync();

    // public async Task CreateSnuffAsync(Snuff newSnuff) =>
    //     await _serviceCollection.InsertOneAsync(newSnuff);

    // public async Task UpdateSnuffAsync(ObjectId id, Snuff updatedSnuff) =>
    //     await _serviceCollection.ReplaceOneAsync(x => x.Id == id, updatedSnuff);

    // public async Task RemoveSnuffAsync(ObjectId id) =>
    //     await _serviceCollection.DeleteOneAsync(x => x.Id == id);


}