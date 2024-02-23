using DAL;
using DAL.Dto;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;
using System.Linq;
using System.Collections.Generic;

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
    public async Task<List<CurrentSnuff>> CreateCurrentSnuffWithDtoAsync(CreateCSDto[] newCurrentSnuff)
    {
        var result = new List<CurrentSnuff>();
        foreach(var item in newCurrentSnuff)
        {
            var getBoxSize = await _snuffService.GetSnuffAmountAsync(item.SnusId);
            if(getBoxSize == null)
            {
                throw new Exception("Snuff not found");
            }
            
            var newSnuff = new CurrentSnuff
            {
                SnusId = item.SnusId,
                PurchaseDate = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
                CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
                UserId = item.UserId,
                LogsOfBox = Array.Empty<SnuffLog>(),
                IsEmpty = false,
                IsArchived = false,
                RemainingAmount = getBoxSize
            };
            await _currentSnuffRepository.InsertOneAsync(newSnuff);
            result.Add(newSnuff);
        }

        return result;
    }

    public async Task<bool> ArchiveAUsersInventoryAsync(string userId)
    {
        var getThisSnuffFromDb =  _currentSnuffRepository.FilterBy(x => x.UserId == userId).ToList();

        if (getThisSnuffFromDb != null)
        {
            foreach (var item in getThisSnuffFromDb)
            {
                if(item.RemainingAmount == 0)
                {
                    item.IsEmpty = true;
                }
                item.IsArchived = true;
                await _currentSnuffRepository.ReplaceOneAsync(item);
            }

            return true;
        }

        return false;
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
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            SnuffLogDate = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
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
    public async Task<CurrentSnuff> LogAdderV2(string csId, int amount)
    {
        //hämta currentsnuff objektet via id
        var currentSnuff = await _currentSnuffRepository.FindOneAsync(x => x.Id == csId);
        if (currentSnuff == null)
        {
            throw new Exception("CurrentSnuff not found");
        }
        //hämta användarens id via currentsnuffid
        var userId = _currentSnuffRepository.FilterBy(x => x.Id == csId).FirstOrDefault()!.UserId;
        if(userId == null)
        {
            throw new Exception("User not found");
        }
        var snuffDefaultAmount = await _snuffService.GetSnuffAmountAsync(currentSnuff.SnusId);
        //skapa en ny logg via CreateSnuffLogAsync
        var createdNewLog = await _snuffLogService.CreateSnuffLogAsync(new SnuffLog
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            SnuffLogDate = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            AmountUsed = amount,
        });
        //lägg till nya loggen i currentsnuff objektet
        var log = currentSnuff.LogsOfBox.Append(createdNewLog).ToArray();
        //kolla om currentsnuff är tom
        var newAmountInBox = snuffDefaultAmount - log.Sum(x => x.AmountUsed);
        //uppdatera currentsnuff objektet
        currentSnuff.RemainingAmount = newAmountInBox;
        if(newAmountInBox <= 0)
        {
            currentSnuff.IsEmpty = true;
        }
        //spara currentsnuff objektet

        var newCurrentSnuff = new CurrentSnuff
        {
            Id = currentSnuff.Id,
            SnusId = currentSnuff.SnusId,
            PurchaseDate = currentSnuff.PurchaseDate,
            CreatedAtUtc = currentSnuff.CreatedAtUtc,
            LogsOfBox = log,
            UserId = currentSnuff.UserId,
            IsEmpty = currentSnuff.IsEmpty,
            IsArchived = currentSnuff.IsArchived,
            RemainingAmount = currentSnuff.RemainingAmount
        };

        await _currentSnuffRepository.ReplaceOneAsync(newCurrentSnuff);

        return newCurrentSnuff;
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

    public async Task<List<CurrentSnuffDto>> GetCurrentSnuffInventoryAsync(string userId)
    {
        var currentSnuffs = _currentSnuffRepository. FilterBy(x => x.UserId == userId && x.IsArchived == false);
        var snuffList = new List<CurrentSnuffDto>();
        foreach (var item in currentSnuffs)
        {
            var snuffObj = await _snuffService.GetSnuffAsync(item.SnusId);
            var snuff = new CurrentSnuffDto
            {
                CurrentSnuffId = item.Id,
                PurchaseDate = item.PurchaseDate,
                AmountRemaing = snuffObj.DefaultAmount - item.LogsOfBox.Sum(x => x.AmountUsed),
                ImageUrl = snuffObj.ImageUrl,
                Type = snuffObj.Type,
                Brand = snuffObj.Brand
            };
            snuffList.Add(snuff);
        }
 
        return snuffList;
    }

    Task ICurrentSnuffService.CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff)
    {
        throw new NotImplementedException();
    }

    Task<CurrentSnuff> ICurrentSnuffService.GetCurrentSnuffAsync(string id)
    {
        throw new NotImplementedException();
    }

    Task ICurrentSnuffService.RemoveCurrentSnuffAsync(string id)
    {
        throw new NotImplementedException();
    }

    Task ICurrentSnuffService.UpdateCurrentSnuffAsync(string id, CurrentSnuff updatedCurrentSnuff)
    {
        throw new NotImplementedException();
    }

    Task<List<CurrentSnuff>> ICurrentSnuffService.GetAllCurrentSnuffsForThisUserAsync(string uid)
    {
        throw new NotImplementedException();
    }

    Task<int> ICurrentSnuffService.GetAmountInBoxAsync(string csid)
    {
        throw new NotImplementedException();
    }

}