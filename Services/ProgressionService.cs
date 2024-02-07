using DAL;
using DAL.Interfaces;
using Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using DAL.Dto;

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

    public async Task<ProgressionDto> AddNewProgressionV2(string uid)
    {
        var selectOldProgression = await _progressionRepository.FindOneAsync(x => x.UserId == uid && x.InUse == true);
        if (selectOldProgression == null)
        {
            var newProgression = await ProgressionHandlerV2(uid);
            await _progressionRepository.InsertOneAsync(newProgression);
            return MapProgressionToDto(newProgression);
        }

        var selectProgressionWithInUseTrue = await FindUserActiveProgression(uid);
        Console.WriteLine("Object Time: " + selectProgressionWithInUseTrue.GoalEndDate);
        Console.WriteLine("Date Time UTC: " + DateTime.UtcNow);
        if (selectProgressionWithInUseTrue.GoalEndDate > DateTime.UtcNow)
        {
            Console.WriteLine("Progression already exists");
            return MapProgressionToDto(selectProgressionWithInUseTrue);
        }
        else if (selectProgressionWithInUseTrue.GoalEndDate < DateTime.UtcNow)
        {
            Console.WriteLine("Progression is old send it to the graveyard(UpdateProgressionStateAsync)");
            var updatedProgression = await UpdateProgressionStateAsync(selectProgressionWithInUseTrue);
            return MapProgressionToDto(updatedProgression);
        }
        return MapProgressionToDto(selectProgressionWithInUseTrue);
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
        if (result == null)
        {
            var checkIfOldProgressionExists = await _progressionRepository.FindOneAsync(x => x.UserId == uid && x.InUse == false);
            if (checkIfOldProgressionExists != null)
            {
                result = await AddNewProgression(uid);
            }
        }
        return result;
    }
    
    public async Task<ProgressionDto> FindUserActiveProgressionDto(string userId)
    {
        var result = null as ProgressionDto;
        var response = await _progressionRepository.FindOneAsync(x => x.UserId == userId && x.InUse == true);
        if (response == null)
        {
            var checkIfOldProgressionExists = await _progressionRepository.FindOneAsync(x => x.UserId == userId && x.InUse == false);
            if (checkIfOldProgressionExists != null)
            {
                var oldProgressionData = await AddNewProgressionV2(userId);
                result = oldProgressionData;
            }
        }
        else
        {
            result = MapProgressionToDto(response);
        }
        return result;

    }

    public ProgressionDto MapProgressionToDto(Progression progression)
    {
        var result = new ProgressionDto
        {
            GoalStartDate = progression.GoalStartDate,
            GoalEndDate = progression.GoalEndDate,
            SnuffLimitAmount = progression.SnuffGoalAmount,
            RecommendedUsageInterval = progression.RecommendedUsageInterval,
            ActualUsageInterval = progression.ActualUsageInterval,
            InUse = progression.InUse
        };
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
    public async Task<Progression> ProgressionHandlerV2(string userId)
    {
        var newProgression = new Progression();
        var habitData = await _habitRepository.FindOneAsync(x => x.UserId == userId);
        var habitDto = new HabitDto
        {
            DoseType = habitData.DoseType,
            DoseAmount = habitData.DoseAmount,
            ProgressionType = habitData.ProgressionType,
            Speed = int.Parse(habitData.Speed),
            NumberOfHoursPerDay = habitData.NumberOfHoursPerDay,
            StartDate = habitData.StartDate,
            EndDate = habitData.EndDate
        };

        var lastprogression = _progressionRepository.FilterBy(x => x.UserId == userId && x.InUse == false).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();
        if (lastprogression == null)
        {
            newProgression = new Progression
            {
                Id = "",
                CreatedAtUtc = DateTime.UtcNow,
                UserId = userId,
                GoalStartDate = DateTime.UtcNow.Date,
                GoalEndDate = DateTime.UtcNow.Date,
                SnuffGoalAmount = habitData.DoseType == "dosor" ? habitData.DoseAmount * 20 - 1 : habitData.DoseAmount - 1,
                RecommendedUsageInterval = new TimeSpan(),
                ActualUsageInterval = new TimeSpan(),
                InUse = true
            };

        }
        else
        {
            newProgression = new Progression
            {
                Id = "",
                CreatedAtUtc = DateTime.UtcNow,
                UserId = userId,
                GoalStartDate = lastprogression.GoalEndDate.AddDays(1),
                GoalEndDate = lastprogression.GoalEndDate.AddDays(1),
                SnuffGoalAmount = lastprogression.SnuffGoalAmount - 1,
                RecommendedUsageInterval = new TimeSpan(),
                ActualUsageInterval = new TimeSpan(),
                InUse = true
            };
        }
        
        switch (habitData.ProgressionType)
        {
            case "app":
                newProgression = await AppProgressionV2(newProgression, habitDto.Speed);
                break;
            case "date":
                newProgression = await DatumProgression(newProgression, habitData);
                break;
            default:
                throw new Exception("Progression type not found");
        }

        newProgression = await SetUsageInterval(newProgression);

        return newProgression;

    }
    public async Task<Progression> ProgressionHandler(string uid)
    {
        var newProgression = new Progression();
        var habitData = await _habitRepository.FindOneAsync(x => x.UserId == uid);

        var lastprogression = _progressionRepository.FilterBy(x => x.UserId == uid && x.InUse == false).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();
        if (lastprogression == null)
        {
            newProgression = new Progression
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

        }
        else
        {
            newProgression = new Progression
            {
                Id = "",
                CreatedAtUtc = DateTime.UtcNow,
                UserId = uid,
                GoalStartDate = lastprogression.GoalEndDate.AddDays(1),
                GoalEndDate = lastprogression.GoalEndDate.AddDays(1),
                SnuffGoalAmount = lastprogression.SnuffGoalAmount - 1,
                RecommendedUsageInterval = new TimeSpan(),
                ActualUsageInterval = new TimeSpan(),
                InUse = true
            };
        }

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
        var mins = spaceBetween % 3600 / 60;
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

    private Task<Progression> AppProgressionV2(Progression appProgressionData, int speed)
    {
        switch (speed)
        {
            case 10:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(2).AddTicks(-1);
                break;
            case 9:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(4).AddTicks(-1);
                break;
            case 8:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(6).AddTicks(-1);
                break;
            case 7:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(8).AddTicks(-1);
                break;
            case 6:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(10).AddTicks(-1);
                break;
            case 5:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(12).AddTicks(-1);
                break;
            case 4:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(14).AddTicks(-1);
                break;
            case 3:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(16).AddTicks(-1);
                break;
            case 2:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(18).AddTicks(-1);
                break;
            case 1:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(20).AddTicks(-1);
                break;
            default:
                appProgressionData.GoalEndDate = appProgressionData.GoalStartDate.AddDays(7).AddTicks(-1);
                break;
        }

        return Task.FromResult(appProgressionData);
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
            Console.WriteLine("Am new TimeInverval " + newTimeInterval);
            Console.WriteLine(TimeSpan.FromSeconds(newTimeInterval) + "From no logs of today");
            progressionDetails.ActualUsageInterval = TimeSpan.FromSeconds(newTimeInterval);
            await _progressionRepository.ReplaceOneAsync(progressionDetails);
            return DateTime.UtcNow.AddSeconds(newTimeInterval).ToString();
        }

        availableSnuffToday = progressionDetails.SnuffGoalAmount - logDetails.Sum(x => x.AmountUsed);
        newTimeInterval = timeLeftOfTheDate.TotalSeconds / availableSnuffToday;
        Console.WriteLine("Am new TimeInverval " + newTimeInterval);
        Console.WriteLine(TimeSpan.FromSeconds(newTimeInterval) + "WITH LOGS of today");
        progressionDetails.ActualUsageInterval = TimeSpan.FromSeconds(newTimeInterval);; //TimeSpan.FromSeconds(newTimeInterval);
        await _progressionRepository.ReplaceOneAsync(progressionDetails);
        Console.WriteLine("ProgressionService Line 216: newTimeInterval: " + newTimeInterval);
        return DateTime.UtcNow.AddSeconds(newTimeInterval).ToString();

    }

    public async Task<TimeSpan> WhenIsTheNextDoseAvailableV2(string uid)
    {

        var usedSnuffToday = await CalculateRemainingSnuff(uid);
        var habit = await _habitRepository.FindOneAsync(x => x.UserId == uid);

        TimeSpan now = DateTime.UtcNow.AddHours(1).TimeOfDay;
        TimeSpan wakeUpTime = (TimeSpan)habit.WakeUpTime;
        TimeSpan bedTime = (TimeSpan)habit.BedTime;

        TimeSpan timeLeft;
        if (now > bedTime || now < wakeUpTime)
        {
            if (now < wakeUpTime)
            {
            timeLeft = wakeUpTime - now;
            }
            else // now > bedTime
            {
            timeLeft = new TimeSpan(24, 0, 0) - now + wakeUpTime; // Time till midnight plus time from midnight to wake up
            }
            
            timeLeft = new TimeSpan(timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
            return timeLeft;
        }
        else
        {
            var TimeLeftToday = bedTime - now;
            var timeToNextSnuff = TimeLeftToday / usedSnuffToday;

            timeToNextSnuff = new TimeSpan(timeToNextSnuff.Hours, timeToNextSnuff.Minutes, timeToNextSnuff.Seconds);
            return timeToNextSnuff;

        }
        // var wakeUpAsDateTime = DateTime.Today + habit.WakeUpTime;
        // var bedTimeAsDateTime = DateTime.Today + habit.BedTime;
        // if(DateTime.UtcNow.AddHours(1) < wakeUpAsDateTime){
        //    var remainingDateTime =  wakeUpAsDateTime - DateTime.UtcNow.AddHours(1);
        //    if (remainingDateTime.HasValue)
        //    {
        //        var result = new TimeSpan(remainingDateTime.Value.Hours, remainingDateTime.Value.Minutes, remainingDateTime.Value.Seconds);
        //        return result;
        //    }
        // }
        // if(DateTime.UtcNow.AddHours(1) > bedTimeAsDateTime){

        //  }
    }

    public async Task<string> LastConsumedSnuffAtUtc(string uid)
    {
        Console.WriteLine("ProgressionService Line 222: uid: " + uid);
        Console.WriteLine("ProgressionService Line 223: DateTime.UtcNow.Date: " + DateTime.UtcNow.Date);
        var allLogByUser = _snuffLogRepository.FilterBy(x => x.SnuffLogDate.Day == DateTime.UtcNow.Day && x.UserId == uid);
        if (allLogByUser == null) // du har inte snusat idag
        {
            Console.WriteLine("ProgressionService Line 227: allLogByUser == null");
            return DateTime.UtcNow.Date.ToString();
        }
        else // du har snusat idag
        {
            var mostRecentSnuff = allLogByUser.OrderByDescending(x => x.SnuffLogDate).FirstOrDefault();
            Console.WriteLine("ProgressionService Line 233: mostRecentSnuff: " + mostRecentSnuff.SnuffLogDate.ToString());
            return mostRecentSnuff.SnuffLogDate.ToString();
        }
    }
}