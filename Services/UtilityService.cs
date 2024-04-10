using DAL;
using DAL.Dto;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Services.Interfaces;

public class UtilityService : IUtilityService
{
    private readonly IGenericMongoRepository<Habit> _habitRepository;
    private readonly IGenericMongoRepository<User> _userRepository;
    private readonly IGenericMongoRepository<CurrentSnuff> _currentsnuffRepository;
    private readonly IGenericMongoRepository<Snuff> _snuffRepository;
    private readonly IHabitService _habitService; 
    private readonly ICurrentSnuffService _currentSnuffService; 
    private readonly ISnuffLogService _snuffLogService; 
    private readonly ISnuffService _snuffService; 
    public UtilityService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<User> userRepository,
            IGenericMongoRepository<Habit> habitRepository,
            IGenericMongoRepository<CurrentSnuff> currentsnuffRepository,
            IGenericMongoRepository<Snuff> snuffRepository,
            IHabitService habitService,
            ICurrentSnuffService currentSnuffService,
            ISnuffLogService snuffLogService,
            ISnuffService snuffService
            )
    {
        _userRepository = userRepository;
        _habitRepository = habitRepository;
        _currentsnuffRepository = currentsnuffRepository;
        _snuffRepository = snuffRepository;
        _habitService = habitService;
        _currentSnuffService = currentSnuffService;
        _snuffLogService = snuffLogService;
        _snuffService = snuffService;
        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }
    public bool IsUserValid(string userId)
    {
        var user = _userRepository.FindOneAsync(x => x.UserId == userId).Result;
        return user != null!;
    }

    public bool UserHasActiveHabit(string userId)
    {
        var habit = _habitRepository.FindOneAsync(x => x.UserId == userId).Result;
        return habit != null; 
    }


    public void RemoveActiveHabit(string userId)
    {
        _habitService.RemoveHabitAsync(userId);
    }

    public Habit CreateNewHabitObejct(FakeUsageData setup)
    {
        var hoursOfDay = setup.BedTime - setup.WakeUpTime;
        var habit = new Habit
        {
            DoseType = "prillor",
            DoseAmount = setup.DoseAmount,
            ProgressionType = "app",
            Speed = setup.Speed.ToString(),
            WakeUpTime = setup.WakeUpTime,
            BedTime = setup.BedTime,
            NumberOfHoursPerDay = hoursOfDay.Hours,
            StartDate = new DateTime(setup.StartDate.Year, setup.StartDate.Month, setup.StartDate.Day),
        };
        return habit;
    }

    public HabitDto CreateHabitDtoObject(FakeUsageData setup)
    {
        var hoursOfDay = setup.BedTime - setup.WakeUpTime;
        var habitDto = new HabitDto
        {
            DoseType = "prillor",
            DoseAmount = setup.DoseAmount,
            ProgressionType = "app",
            Speed = setup.Speed,
            WakeUpTime = setup.WakeUpTime,
            BedTime = setup.BedTime,
            NumberOfHoursPerDay = hoursOfDay.Hours,
            StartDate = new DateTime(setup.StartDate.Year, setup.StartDate.Month, setup.StartDate.Day),
        };
        return habitDto;
    }

    public List<Snuff> GetListOfRecommendedSnuffSort(string userId, double avgNicotinePerPortion)
    {
        var minNicotine = avgNicotinePerPortion * 0.8;
        var maxNicotine = avgNicotinePerPortion * 1.2;
        var listOfAllSnus = _snuffRepository.FilterBy(x => x.SnuffInfo!.NicotinePerPortion >= minNicotine && x.SnuffInfo.NicotinePerPortion <= maxNicotine).ToList();
        return listOfAllSnus;
    }

    public async void BuySnuff(string snuffId, string userId, DateTime purchaseDate)
    {
        var currentSnuff = new CurrentSnuff
        {
            SnusId = snuffId,
            PurchaseDate = purchaseDate,
            UserId = userId,
            IsEmpty = false,
            IsArchived = false,
            RemainingAmount = 1
        };
       await _currentSnuffService.CreateCurrentSnuffAsync(currentSnuff);
    }

    public async void MakeLogOfUsage(string userId, string csId, int amount, DateTime date)
    {
        var currentSnuff = _currentsnuffRepository.FindOneAsync(x => x.UserId == userId && x.Id == csId).Result;
        if (currentSnuff == null)
        {
            throw new Exception("Current snuff does not exist");
        }
        if (currentSnuff.IsEmpty)
        {
            throw new Exception("Current snuff is empty");
        }
        if (currentSnuff.IsArchived)
        {
            throw new Exception("Current snuff is archived");
        }
        if (currentSnuff.RemainingAmount < amount)
        {
            throw new Exception("Not enough snuff left");
        }
        var snuffLog = new SnuffLog
        {
            CreatedAtUtc = date,
            UserId = userId,
            AmountUsed = amount,
            SnuffLogDate = date
        };
        var snuffDefaultAmount = await _snuffService.GetSnuffAmountAsync(currentSnuff.SnusId);
        var log = currentSnuff.LogsOfBox.Append(snuffLog).ToArray();
        var replaceCurrentSnuff = new CurrentSnuff
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
        await _snuffLogService.CreateSnuffLogAsync(snuffLog);
        await _currentsnuffRepository.ReplaceOneAsync(replaceCurrentSnuff);


        
    }

    public void GenerateFakeUsageDate(FakeUsageData fakeUsageData)
    {
        var userIdValid = IsUserValid(fakeUsageData.UserId);
        if (!userIdValid)
        {
            throw new Exception("User does not exist");
        }
        var userHasActiveHabit = UserHasActiveHabit(fakeUsageData.UserId);
        if (userHasActiveHabit)
        {
            RemoveActiveHabit(fakeUsageData.UserId);
        }
        var habit = CreateNewHabitObejct(fakeUsageData);
        _habitRepository.InsertOneAsync(habit);
        var habitDto = CreateHabitDtoObject(fakeUsageData);
        _habitService.SetRulesForUsersProgression(habitDto, fakeUsageData.UserId);
    }


}