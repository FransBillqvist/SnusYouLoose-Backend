using DAL;
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
        return user != null;
    }

    public bool UserHasActiveHabit(string userId)
    {
        var habit = _habitRepository.FindOneAsync(x => x.UserId == userId).Result;
        return habit != null; 
    }

    //om UserHasActiveHabit == true, remove habit

    public void RemoveActiveHabit(string userId)
    {
        _habitService.RemoveHabitAsync(userId);
    }


}