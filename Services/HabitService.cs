using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class HabitService : IHabitService
{
    private readonly IGenericMongoRepository<Habit> _habitRepository;

    public HabitService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<Habit> habitRepository
        )
    {
        _habitRepository = habitRepository;
        var mongoClient = new MongoClient(
            Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }

    public async Task<Habit> GetHabitAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        return await _habitRepository.FindOneAsync(x => x.Id == id);
    }

    public async Task CreateHabitAsync(Habit newHabit) =>
        await _habitRepository.InsertOneAsync(newHabit);

    public async Task UpdateHabitAsync(string id, Habit updatedHabit)
    {
        await _habitRepository.ReplaceOneAsync(updatedHabit);
    }

    public async Task RemoveHabitAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _habitRepository.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<Habit> SetEndDateForHabit(Habit dto)
    {
        var habit = await _habitRepository.FindOneAsync(x => x.Id == dto.Id);
        var speed = habit.Speed;
        var days = 0;
        switch (speed)
        {
            case "Långsam":
                days = 4;
                break;
            case "Lagom":
                days = 7;
                break;
            case "Snabbt":
                days = 14;
                break;
            default:
                days = 7;
                break;
        }
        for (int i = 0; i < habit.DoseAmount; i++)
        {
            habit.EndDate = habit.EndDate.AddDays(days);
        }

        await _habitRepository.ReplaceOneAsync(habit);
        return habit;
    }
}