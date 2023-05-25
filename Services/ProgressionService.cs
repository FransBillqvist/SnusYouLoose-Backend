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

    public async Task<Progression> AddNewProgression(string uid)
    {
        var selectOldProgression = await _progressionRepository.FindOneAsync(x => x.UserId == uid && x.InUse == true);
        if (selectOldProgression == null)
        {
            var newProgression = await ProgressionHandler(uid);
            await _progressionRepository.InsertOneAsync(newProgression);
            return newProgression;
        }

        var selectProgressionWithInUseTrue = await FindUserActiveProgression(uid);
        Console.WriteLine("Object Time: " + selectProgressionWithInUseTrue.GoalEndDate);
        Console.WriteLine("Date Time UTC: " + DateTime.UtcNow);
        if (selectProgressionWithInUseTrue.GoalEndDate > DateTime.UtcNow)
        {
            Console.WriteLine("Progression already exists");
            return selectProgressionWithInUseTrue;
        }
        else if (selectProgressionWithInUseTrue.GoalEndDate < DateTime.UtcNow)
        {
            Console.WriteLine("Progression is old send it to the graveyard(UpdateProgressionStateAsync)");
            var updatedProgression = await UpdateProgressionStateAsync(selectProgressionWithInUseTrue);
            return updatedProgression;
        }
        return selectProgressionWithInUseTrue;
    }

    public async Task<int> CalculateRemainingSnuff(string uid)
    {
        var allLogsForUserToday = _snuffLogRepository.FilterBy(x => x.SnuffLogDate.Day == DateTime.UtcNow.Day && x.UserId == uid);
        var activeProgression = await _progressionRepository.FindOneAsync(x => x.UserId == uid && x.InUse == true);
        var numberOfUsedSnuff = 0;
        foreach (var log in allLogsForUserToday)
        {
            numberOfUsedSnuff += log.AmountUsed;
        }
        var remainingSnuff = activeProgression.SnuffGoalAmount - numberOfUsedSnuff;
        return remainingSnuff;
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

    public async Task<Progression> UpdateProgressionStateAsync(Progression updatedProgression)
    {
        updatedProgression.InUse = false;
        await _progressionRepository.ReplaceOneAsync(updatedProgression);
        await AddNewProgression(updatedProgression.UserId);
        return updatedProgression;
    }

    public async Task<Progression> ProgressionHandler(string uid)
    {
        var habitData = await _habitRepository.FindOneAsync(x => x.UserId == uid);

        var newProgression = new Progression
        {
            Id = "",
            CreatedAtUtc = DateTime.UtcNow,
            UserId = uid,
            GoalStartDate = DateTime.UtcNow.Date,
            GoalEndDate = DateTime.UtcNow.Date,
            SnuffGoalAmount = habitData.DoseType == "dosor" ? habitData.DoseAmount * 20 - 1 : habitData.DoseAmount - 1,
            RecommendedUsageInterval = new TimeSpan(),
            ActualUsageInterval = new TimeSpan(),
            InUse = true
        };
        Console.WriteLine("Before Switch(ProgressionType)");
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
        Console.WriteLine("Switch Switch(ProgressionType)");

        newProgression = await SetUsageInterval(newProgression);

        return newProgression;
    }

    private async Task<Progression> SetUsageInterval(Progression progressionData)
    {
        var habitValue = await _habitRepository.FindOneAsync(x => x.UserId == progressionData.UserId);

        if (progressionData.SnuffGoalAmount == 0)
        {
            progressionData.SnuffGoalAmount = habitValue.DoseAmount;
        }
        var lazyveriable = habitValue.NumberOfHoursPerDay;

        var spaceBetween = lazyveriable * 60 * 60 / progressionData.SnuffGoalAmount;
        Console.WriteLine("Space between: " + spaceBetween);
        //conver space between to hours, mins and seconds
        var hours = spaceBetween / 3600;
        var mins = (spaceBetween % 3600) / 60;
        var seconds = spaceBetween % 60;
        var valueInTimeSpan = new TimeSpan(hours, mins, seconds);
        Console.WriteLine("TimeSpan: " + valueInTimeSpan);

        progressionData.RecommendedUsageInterval = valueInTimeSpan;
        progressionData.ActualUsageInterval = valueInTimeSpan;

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

    public async Task<string> WhenIsTheNextDoseAvailable(string uid)
    {
        var newTimeInterval = 0.0;
        var availableSnuffToday = 0;
        var usedSnuffToday = await CalculateRemainingSnuff(uid);
        var timeLeftOfTheDate = DateTime.UtcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59) - DateTime.UtcNow;

        var logDetails = _snuffLogRepository.FilterBy(x => x.SnuffLogDate.Day == DateTime.UtcNow.Day && x.UserId == uid);
        var progressionDetails = _progressionRepository.FilterBy(x => x.UserId == uid && x.InUse == true).FirstOrDefault();

        if (logDetails == null)
        {
            availableSnuffToday = progressionDetails.SnuffGoalAmount;
            newTimeInterval = timeLeftOfTheDate.TotalSeconds / availableSnuffToday;
            Console.WriteLine(TimeSpan.FromSeconds(newTimeInterval) + "From no logs of today");
            progressionDetails.ActualUsageInterval = TimeSpan.FromSeconds(newTimeInterval);
            await _progressionRepository.ReplaceOneAsync(progressionDetails);
            return newTimeInterval.ToString();
        }

        availableSnuffToday = progressionDetails.SnuffGoalAmount - logDetails.Sum(x => x.AmountUsed);
        newTimeInterval = timeLeftOfTheDate.TotalSeconds / availableSnuffToday;
        Console.WriteLine(TimeSpan.FromSeconds(newTimeInterval) + "With logs of today");
        progressionDetails.ActualUsageInterval = TimeSpan.FromSeconds(newTimeInterval);
        await _progressionRepository.ReplaceOneAsync(progressionDetails);
        return newTimeInterval.ToString();

    }

    public async Task<TimeSpan> LastConsumedSnuffAtUtc(string uid)
    {
        var lastLog = _snuffLogRepository.FilterBy(x => x.UserId == uid).OrderByDescending(x => x.SnuffLogDate).FirstOrDefault();
        if (lastLog == null)
        {
            var todaysdate = long.Parse(DateTime.UtcNow.Date.ToString());
            var timeNow = long.Parse(DateTime.UtcNow.ToString());
            var mathTime = timeNow - todaysdate;
            return new TimeSpan(mathTime);
        }
        else
        {
            var convertDateToLong = long.Parse(DateTime.UtcNow.ToString());
            var convertLogDateToLong = long.Parse(lastLog.SnuffLogDate.ToString());
            var convertToLong = convertDateToLong - convertLogDateToLong;
            return new TimeSpan(convertToLong);
        }
    }
}