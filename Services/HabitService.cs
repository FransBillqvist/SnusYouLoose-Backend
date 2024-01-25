using DAL;
using DAL.Dto;
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
        Console.WriteLine("Hello, Am Get Habit Service with incoming string: " + id);
        // ObjectId mongoId = ObjectId.Parse(id);
        // Console.WriteLine("Hello, Am Get Habit Service with Converted ObjectId: " + mongoId);
        return await _habitRepository.FindOneAsync(x => x.UserId == id);
    }

    public async Task CreateHabitAsync(Habit newHabit)
    {
        newHabit.StartDate = DateTime.UtcNow;
        newHabit.EndDate = DateTime.UtcNow.AddTicks(1);
        await _habitRepository.InsertOneAsync(newHabit);
        await SetEndDateForHabitWithoutEndDate(newHabit);
    }
    public async Task UpdateHabitAsync(string id, Habit updatedHabit)
    {
        await _habitRepository.ReplaceOneAsync(updatedHabit);
    }

    public async Task RemoveHabitAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _habitRepository.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<Habit> SetEndDateForHabitWithoutEndDate(Habit dto)
    {
        var dosorAmount = (dto.DoseType == "dosor" ? dto.DoseAmount * 20 : dto.DoseAmount);

        var speed = dto.Speed;
        var days = 0;
        switch (speed)
        {
            case "Snabbt":
                days = 4;
                break;
            case "Lagom":
                days = 7;
                break;
            case "Långsamt":
                days = 14;
                break;
            default:
                days = 7;
                break;
        }
        for (int i = 0; i < dosorAmount; i++)
        {
            dto.EndDate = dto.EndDate.AddDays(days);
        }

        await _habitRepository.ReplaceOneAsync(dto);
        return dto;
    }

    public async Task<HabitDto> GetHabitDtoAsync(string id) 
    {
        var findHabit = await _habitRepository.FindOneAsync(x => x.UserId == id);
        var habitDto = new HabitDto
        {
            DoseType = findHabit.DoseType,
            DoseAmount = findHabit.DoseAmount,
            ProgressionType = findHabit.ProgressionType,
            Speed = findHabit.Speed,
            NumberOfHoursPerDay = findHabit.NumberOfHoursPerDay,
            StartDate = findHabit.StartDate,
            EndDate = findHabit.EndDate
        };

        return habitDto;
    }

    public async Task<HabitDto> CreateHabitFromRequestAsync(HabitDto newHabit, string userId)
    {
        if(newHabit.EndDate != new DateTime(0)){

        var habit = new Habit
        {
            UserId = userId,
            DoseType = newHabit.DoseType,
            DoseAmount = newHabit.DoseAmount,
            ProgressionType = newHabit.ProgressionType,
            Speed = newHabit.Speed,
            NumberOfHoursPerDay = newHabit.NumberOfHoursPerDay,
            StartDate = DateTime.UtcNow,
            EndDate = newHabit.EndDate
        };
        await _habitRepository.InsertOneAsync(habit);
        // await SetEndDateForHabitWithoutEndDate(habit);
        var result = await GetHabitDtoAsync(userId);
        return result;
        }
        else{
            var habit = new Habit
        {
            UserId = userId,
            DoseType = newHabit.DoseType,
            DoseAmount = newHabit.DoseAmount,
            ProgressionType = newHabit.ProgressionType,
            Speed = newHabit.Speed,
            NumberOfHoursPerDay = newHabit.NumberOfHoursPerDay,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddTicks(1)
        };
        await _habitRepository.InsertOneAsync(habit);
        await SetEndDateForHabitWithoutEndDate(habit);
        var result = await GetHabitDtoAsync(userId);
        return result;
    }
}}