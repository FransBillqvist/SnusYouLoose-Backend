using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class SnuffLogService : ISnuffLogService
{
    private readonly IGenericMongoRepository<SnuffLog> _snuffLogRepository;
    public SnuffLogService(
            IOptions<MongoDbSettings> Settings, IGenericMongoRepository<SnuffLog> snuffLogRepository)
    {
        _snuffLogRepository = snuffLogRepository;

        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }
    public async Task<SnuffLog> CreateSnuffLogAsync(SnuffLog newSnuffLog)
    {
        try
        {
            await _snuffLogRepository.InsertOneAsync(newSnuffLog);
            return newSnuffLog;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    public async Task<SnuffLog> GetSnuffLogAsync(string id) =>
        await _snuffLogRepository.FindOneAsync(x => x.Id == id);

    public async Task UpdateSnuffLogAsync(ObjectId id, SnuffLog updatedSnuffLog) =>
        await _snuffLogRepository.ReplaceOneAsync(updatedSnuffLog);

    public async Task RemoveSnuffLogAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _snuffLogRepository.DeleteOneAsync(x => x.Id == id);
    }
}