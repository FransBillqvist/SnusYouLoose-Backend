using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class CurrentSnuffService : ICurrentSnuffService
{
    private readonly IGenericMongoRepository<CurrentSnuff> _currentSnuffRepository;
    private readonly ISnuffLogService _snuffLogService;
    private readonly ISnuffService _snuffService;

    public CurrentSnuffService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<CurrentSnuff> currentSnuffRepository,
            ISnuffLogService snuffLogService,
            ISnuffService snuffService
            )
    {
        _snuffLogService = snuffLogService;
        _snuffService = snuffService;
        _currentSnuffRepository = currentSnuffRepository;

        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }

    public async Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff)
    {
        await _currentSnuffRepository.InsertOneAsync(newCurrentSnuff);
    }

    public async Task<CurrentSnuff> GetCurrentSnuffAsync(string id)
    {
        var result = await _currentSnuffRepository.FindOneAsync(x => x.Id == id);

        if (result == null)
        {
            Console.WriteLine("CurrentSnuff not found");
        }

        return result;
    }

    public async Task<Boolean> ReturnEmptyStatus(SnuffLog[] Logs, string snuffId)
    {
        if (Logs != null)
        {
            var numberOfUsedSnuff = 0;
            foreach (var snuff in Logs)
            {
                numberOfUsedSnuff += snuff.AmountUsed;
            }

            var getDefaultSnuffAmount = await _snuffService.GetSnuffAmountAsync(snuffId);
            return getDefaultSnuffAmount == numberOfUsedSnuff ? true : false;
        }
        return false;
    }
    public async Task<CurrentSnuff> LogAdder(string id, int amount, string userId)
    {
        //skapa en log via snufflogservice
        var createdNewLog = await _snuffLogService.CreateSnuffLogAsync(new SnuffLog
        {
            UserId = userId,
            SnuffLogDate = DateTime.UtcNow,
            AmountUsed = amount,
        });

        var currentSnuff = await _currentSnuffRepository.FindOneAsync(x => x.Id == id);
        if (currentSnuff != null) //finns dosan?
        {
            var log = currentSnuff.LogsOfBox.Append(createdNewLog).ToArray();
            currentSnuff.LogsOfBox = log;
            currentSnuff.IsEmpty = await ReturnEmptyStatus(log, currentSnuff.SnusId);

            await _currentSnuffRepository.ReplaceOneAsync(currentSnuff);
            return currentSnuff;
        }
        else
        {
            throw new Exception("CurrentSnuff not found");
        }

    }

    public async Task UpdateCurrentSnuffAsync(string id, CurrentSnuff updatedCurrentSnuff) =>
        await _currentSnuffRepository.ReplaceOneAsync(updatedCurrentSnuff);

    public async Task RemoveCurrentSnuffAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _currentSnuffRepository.DeleteOneAsync(x => x.Id == id);
    }
}