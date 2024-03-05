using DAL.Dto;
using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;
using DAL.Enums;

namespace Services;

public class SnuffService : ISnuffService
{
    private readonly IGenericMongoRepository<Snuff> _snuffRepository;
    private readonly IGenericMongoRepository<SnuffInfo> _snuffInfoRepository;

    public SnuffService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<Snuff> snuffRepository,
            IGenericMongoRepository<SnuffInfo> snuffInfoRepository
        )
    {
        _snuffRepository = snuffRepository;
        _snuffInfoRepository = snuffInfoRepository;
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

    public async Task<SnuffShopDto> GetSnuffViaIdAsync(string id) {
      var getItAsSnuff =  await _snuffRepository.FindOneAsync(x => x.Id == id);
        var snuffShopDto = new SnuffShopDto
        {
            Id = getItAsSnuff.Id,
            Brand = getItAsSnuff.Brand,
            Type = getItAsSnuff.Type,
            Price = getItAsSnuff.Price,
            DefaultAmount = getItAsSnuff.DefaultAmount,
            ImageUrl = getItAsSnuff.ImageUrl
        };

        return snuffShopDto;

    }

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

    public async Task<List<SnuffShopDto>> GetShopSnuffsAsync()
    {
        var result = new List<SnuffShopDto>();
        var response = _snuffRepository.FilterBy(x => x.Id != null);
        //konvertera till SnuffShopDto

        foreach (var item in response)
        {
            var snuffShopDto = new SnuffShopDto
            {
                Id = item.Id,
                Brand = item.Brand,
                Type = item.Type,
                Price = item.Price,
                DefaultAmount = item.DefaultAmount,
                ImageUrl = item.ImageUrl
            };
            result.Add(snuffShopDto);
        }
        return result;
        
    }

    public async Task AddInfoToSnuffAsync(SnuffInfoReq snuffInfo)
    {
        var snuff = await _snuffRepository.FindOneAsync(x => x.Id == snuffInfo.SnusId);
        if(snuff is null)
        {
            throw new Exception("Snuff not found");
        }

        var flavorList = new List<Flavor>();
        foreach (var item in snuffInfo.Flavors)
        {
            flavorList.Add((Flavor)item);
        }
        
        var result = new SnuffInfo
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            SnusId = snuffInfo.SnusId,
            NicotinePerGram = snuffInfo.NicotinePerGram,
            NicotinePerPortion = snuffInfo.NicotinePerPortion,
            Flavors = flavorList,
            Format = snuffInfo.Format
        };
        
        await _snuffInfoRepository.InsertOneAsync(result);
    }

    public async Task<List<Snuff>> GetAllSnuffWithInfoAsync()
    {
        var response = _snuffRepository.FilterBy(x => x.Id != null).ToList();
        var result = new List<Snuff>();

        foreach (var item in response)
        {
            var snuffInfo = await _snuffInfoRepository.FindOneAsync(x => x.SnusId == item.Id);
            if (snuffInfo is not null)
            {
                item.SnuffInfo = snuffInfo;
                result.Add(item);
            }
            else
            {
                result.Add(item);
            }
        }
        return response;
    }
}