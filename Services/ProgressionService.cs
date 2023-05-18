using DAL;
using DAL.Interfaces;
using Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace Services;

public class ProgressionService : IProgressionService
{
    private readonly IGenericMongoRepository<Progression> _progressionRepository;
    private readonly IGenericMongoRepository<User> _userRepository;
    private readonly IGenericMongoRepository<SnuffLog> _snuffLogRepository;
    private readonly IGenericMongoRepository<Habit> _habitRepository;
    private readonly ILogger<ProgressionService> _logger;

    public ProgressionService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<Progression> progressionRepository,
            IGenericMongoRepository<User> userRepository,
            IGenericMongoRepository<SnuffLog> snuffLogRepository,
            IGenericMongoRepository<Habit> habitRepository,
            ILogger<ProgressionService> logger)
    {
        _progressionRepository = progressionRepository;
        _userRepository = userRepository;
        _snuffLogRepository = snuffLogRepository;
        _habitRepository = habitRepository;
        _logger = logger;

        var mongoClient = new MongoClient(
             Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }

    public async Task AddNewProgression(string uid)
    {
        var selectOldProgression = await _progressionRepository.FindOneAsync(x => x.UserId == uid);
        if (selectOldProgression == null)
        {
            var newProgression = await ProgressionHandler(uid);
            await _progressionRepository.InsertOneAsync(newProgression);
        }

        var selectProgressionWithInUseTrue = await FindUserActiveProgression(uid);

        if (selectProgressionWithInUseTrue.GoalEndDate > DateTime.Now)
        {
            Console.WriteLine("Progression already exists");
        }
        else if (selectProgressionWithInUseTrue.GoalEndDate < DateTime.Now)
        {
            await UpdateProgressionStateAsync(selectProgressionWithInUseTrue);
        }

    }

    public async Task<int> CalculateRemainingSnuff(string uid)
    {
        var allLogsForUserToday = _snuffLogRepository.FilterBy(x => x.SnuffLogDate == DateTime.Now.Date && x.UserId == uid);
        var activeProgression = await _progressionRepository.FindOneAsync(x => x.UserId == uid && x.InUse == true);
        var numberOfSnuffLogs = 0;
        foreach (var log in allLogsForUserToday)
        {
            numberOfSnuffLogs += log.AmountUsed;
        }
        return activeProgression.SnuffGoalAmount - numberOfSnuffLogs;
    }

    public async Task<Progression> FindUserActiveProgression(string uid)
    {
        var result = await _progressionRepository.FindOneAsync(x => x.UserId == uid && x.InUse == true);
        return result;
    }

    public async Task RemoveProgressionAsync(string pid)
    {
        var currentProgression = await _progressionRepository.FindOneAsync(x => x.Id == pid && x.InUse == true);
        if (currentProgression == null)
        {
            throw new Exception("No progression found");
        }
        else
        {
            await _progressionRepository.DeleteOneAsync(x => x.Id == pid);
        }
    }

    public async Task UpdateProgressionStateAsync(Progression updatedProgression)
    {
        updatedProgression.InUse = false;
        await _progressionRepository.ReplaceOneAsync(updatedProgression);
        await AddNewProgression(updatedProgression.UserId);
    }

    public async Task<Progression> ProgressionHandler(string uid)
    {
        var habitData = await _habitRepository.FindOneAsync(x => x.UserId == uid);

        var newProgression = new Progression
        {
            Id = "",
            UserId = uid,
            GoalStartDate = DateTime.Now.Date,
            GoalEndDate = DateTime.Now.Date,
            SnuffGoalAmount = (habitData.DoseType == "dosor" ? habitData.DoseAmount * 20 - 1 : habitData.DoseAmount - 1),
            UsageInterval = new TimeSpan(),
            InUse = true
        };

        switch (habitData.ProgressionType)
        {
            case "app":
                newProgression = await AppProgression(newProgression, habitData.Speed);
                break;
            case "datum":
                newProgression = await DatumProgression(newProgression, habitData);
                break;
            default:
                throw new Exception("Progression type not found");
        }

        newProgression = await SetUsageInterval(newProgression);

        return newProgression;
    }

    private async Task<Progression> SetUsageInterval(Progression progressionData)
    {

        var spaceBetween = progressionData.SnuffGoalAmount / 24;

        var hours = spaceBetween / 60;
        var minutes = spaceBetween % 60;
        var seconds = 0;
        var valueInTimeSpan = new TimeSpan(hours, minutes, seconds);

        progressionData.UsageInterval = valueInTimeSpan;

        return progressionData;
    }

    private async Task<Progression> AppProgression(Progression appProgressionData, string speed)
    {
        switch (speed)
        {
            case "Snabbt":
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(4).AddTicks(-1);
                break;
            case "Lagom":
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(7).AddTicks(-1);
                break;
            case "Långsamt":
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(12).AddTicks(-1);
                break;
            default:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(7).AddTicks(-1);
                break;
        }
        return appProgressionData;
    }
    private async Task<Progression> DatumProgression(Progression datumProgressionData, Habit speed)
    {
        //TODO: Implement DatumProgression
        return datumProgressionData;
    }

}