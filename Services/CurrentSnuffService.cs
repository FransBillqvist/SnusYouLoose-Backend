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
        var getBoxSize = await _snuffService.GetSnuffAmountAsync(newCurrentSnuff.SnusId);
        newCurrentSnuff.RemainingAmount = getBoxSize;
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
            if (numberOfUsedSnuff >= getDefaultSnuffAmount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public async Task<CurrentSnuff> LogAdder(string id, int amount, string userId)
    {
        //skapa en log via snufflogservice
        var createdNewLog = await _snuffLogService.CreateSnuffLogAsync(new SnuffLog
        {
            CreatedAtUtc = DateTime.UtcNow,
            UserId = userId,
            SnuffLogDate = DateTime.UtcNow,
            AmountUsed = amount,
        });
        var yolo = _currentSnuffRepository.FilterBy(x => x.SnusId == id && x.UserId == userId);
        var yoloId = yolo.FirstOrDefault().Id;
        var currentSnuff = await _currentSnuffRepository.FindOneAsync(x => x.Id == yoloId);
        if (currentSnuff != null) //finns dosan?
        {
            Console.WriteLine("I am here and my id is: " + id);
            var log = currentSnuff.LogsOfBox.Append(createdNewLog).ToArray();
            currentSnuff.LogsOfBox = log;
            var testBoolResult = await ReturnEmptyStatus(log, currentSnuff.SnusId);
            currentSnuff.IsEmpty = testBoolResult;
            currentSnuff.IsArchived = (currentSnuff.IsEmpty ? true : false);
            var testRemainingAmount = await GetAmountInBoxAsync(yoloId);
            Console.WriteLine("I am remaining amount" + testRemainingAmount);
            currentSnuff.RemainingAmount = testRemainingAmount;
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

    public async Task<List<CurrentSnuff>> GetAllCurrentSnuffsForThisUserAsync(string uid)
    {
        try
        {
            return _currentSnuffRepository.FilterBy(x => x.UserId == uid && x.IsEmpty == false).ToList<CurrentSnuff>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<bool> AddCurrentSnuffToArchiveAsync(string CurrentSnuffId)
    {
        var getThisSnuffFromDb = await _currentSnuffRepository.FindOneAsync(x => x.Id == CurrentSnuffId);

        if (getThisSnuffFromDb != null)
        {
            getThisSnuffFromDb.IsArchived = true;
            await _currentSnuffRepository.ReplaceOneAsync(getThisSnuffFromDb);

            return true;
        }

        return false;
    }

    public async Task<int> GetAmountInBoxAsync(string csObjectId)
    {
        var getThisSnuffFromDb = await _currentSnuffRepository.FindOneAsync(x => x.Id == csObjectId);
        var boxSize = _snuffService.GetSnuffAmountAsync(getThisSnuffFromDb.SnusId).Result;

        if (getThisSnuffFromDb == null)
        {
            return 0;
        }
        var numberofused = 0;
        foreach (var log in getThisSnuffFromDb.LogsOfBox)
        {
            numberofused += log.AmountUsed;
        }

        Console.WriteLine("Time to get outta here" + numberofused);
        getThisSnuffFromDb.RemainingAmount = boxSize - numberofused;
        return boxSize - numberofused;
    }
}