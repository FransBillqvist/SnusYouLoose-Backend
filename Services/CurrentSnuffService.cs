using DAL;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class CurrentSnuffService : ICurrentSnuffService
{
    private readonly IGenericMongoRepository<CurrentSnuff> _currentSnuffRepository;
    private readonly ISnuffLogService _snuffLogService;

    public CurrentSnuffService(
        IOptions<MongoDbSettings> Settings, IGenericMongoRepository<CurrentSnuff> currentSnuffRepository, ISnuffLogService snuffLogService)
    {

        _snuffLogService = snuffLogService;

        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);

        _currentSnuffRepository = currentSnuffRepository;
    }

    public Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff)
    {
        throw new NotImplementedException();
    }

    public async Task<CurrentSnuff> GetCurrentSnuffAsync(string id)
    {
        var result = await _currentSnuffRepository.FindOneAsync(x => x.Id == id);

        if (result == null)
        {
            throw new Exception("CurrentSnuff not found");
        }

        return result;
    }

    public async Task<CurrentSnuff> UpdateCurrentSnuffAsync(string id, CurrentSnuff updatedCurrentSnuff)
    {
        var currentSnuff = await _currentSnuffRepository.FindByIdAsync(id);

        if (currentSnuff is null)
        {
            throw new Exception("CurrentSnuff not found");
        }

        if (updatedCurrentSnuff.CurrentAmount == 0)
        {
            updatedCurrentSnuff.IsEmpty = true;
        }

        updatedCurrentSnuff.Id = currentSnuff.Id;

        await _currentSnuffRepository.ReplaceOneAsync(updatedCurrentSnuff);

        await _snuffLogService.CreateSnuffLogAsync(new SnuffLog
        {
            UserId = "ABABABA",
            CurrentSnusId = id,
            SnuffLogDate = DateTime.UtcNow,
            Amount = currentSnuff.CurrentAmount - updatedCurrentSnuff.CurrentAmount,
        });

        return updatedCurrentSnuff;

    }

    // public async Task<List<CurrentSnuff>> GetAllCurrentSnuffAsync() => 
    //     await _currentSnuffRepository.Find(_ => true).ToListAsync();

    // public async Task<CurrentSnuff> GetCurrentSnuffAsync(ObjectId id) =>
    //     await _currentSnuffRepository.Find(x => x.Id == id).FirstOrDefaultAsync();

    // public async Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff) =>
    //     await _currentSnuffRepository.InsertOneAsync(newCurrentSnuff);

    // public async Task UpdateCurrentSnuffAsync(ObjectId id, CurrentSnuff updatedCurrentSnuff) =>
    //     await _currentSnuffRepository.ReplaceOneAsync(x => x.Id == id, updatedCurrentSnuff);

    // public async Task RemoveCurrentSnuffAsync(ObjectId id) =>
    //     await _currentSnuffRepository.DeleteOneAsync(x => x.Id == id);
}