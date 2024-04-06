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
    private readonly IHabitService _habitService; 
    public UtilityService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<User> userRepository,
            IGenericMongoRepository<Habit> habitRepository,
            IHabitService habitService)
    {
        _userRepository = userRepository;
        _habitRepository = habitRepository;
        _habitService = habitService;
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

    // public void CreateEndDateForHabit(HabitDto habit, string userId)
    // {
    //     _habitService.SetRulesForUsersProgression(habit, userId);
    // }
        
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