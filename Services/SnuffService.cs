using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class SnuffService : ISnuffService
{
    private readonly IGenericMongoRepository<Snuff> _snuffRepository;

    public SnuffService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<Snuff> snuffRepository
        )
    {
        _snuffRepository = snuffRepository;
        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }

    public async Task<Snuff> CreateSnuffAsync(Snuff newSnuff)
    {
        try
        {
            await _snuffRepository.InsertOneAsync(newSnuff);
            return newSnuff;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

     public async Task<Snuff> CreateSnuffAsyncV2(Snuff newSnuff)
    {
        try
        {
            Console.WriteLine($"Create Snuff data for Snuff(Sevice): {newSnuff.Id}");
            await _snuffRepository.InsertOneAsync(newSnuff);
            return newSnuff;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create Snuff data for Snuff(Sevice): {ex.Message}");
            throw new Exception(ex.Message);
        }

    }
    public async Task<int> GetSnuffAmountAsync(string snuffId)
    {
        var response = await _snuffRepository.FindOneAsync(x => x.Id == snuffId);
        return (response != null ? response.DefaultAmount : 0);
    }

    public async Task<Snuff> GetSnuffAsync(string id) =>
        await _snuffRepository.FindOneAsync(x => x.Id == id);

    public async Task UpdateSnuffAsync(string id, Snuff updatedSnuff) =>
        await _snuffRepository.ReplaceOneAsync(updatedSnuff);

    public async Task RemoveSnuffAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _snuffRepository.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<List<Snuff>> GetAllSnuffsAsync()
    {
        var response = _snuffRepository.FilterBy(x => x.Id != null);
        return response.ToList();
    }
}