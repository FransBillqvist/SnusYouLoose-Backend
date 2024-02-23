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
        newHabit.StartDate = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1);
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

    public async Task<HabitDto> SetRulesForUsersProgression(HabitDto habit,  string userId)
    {
        var dosorAmount = habit.DoseType == "dosor" ? habit.DoseAmount * 20 : habit.DoseAmount;
        var habitById = await _habitRepository.FindOneAsync(x => x.UserId == userId);

        var selectedMode = habit.ProgressionType;
        if(selectedMode == "app"){
            var days = 0;
            switch (habit.Speed)
            {
                case 1:
                    days = 20;
                    break;
                case 2:
                    days = 18;
                    break;
                case 3:
                    days = 16;
                    break;
                case 4:
                    days = 14;
                    break;
                case 5:
                    days = 12;
                    break;
                case 6:
                    days = 10;
                    break;
                case 7:
                    days = 8;
                    break;
                case 8:
                    days = 6;
                    break;
                case 9:
                    days = 4;
                    break;
                case 10:
                    days = 2;
                    break;
                default:
                    days = 7;
                    break;
            }

             for (int i = 0; i < dosorAmount; i++)
        {
            habit.EndDate = habit.EndDate.AddDays(days);
        }
        
            habitById.EndDate = habit.EndDate;
            await _habitRepository.ReplaceOneAsync(habitById);
        return habit;

        }
        if(selectedMode == "date"){
            var daysLeft = habit.EndDate - (DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1));
            var days = daysLeft.Days / dosorAmount;
            while(days < 2.1){
                dosorAmount = dosorAmount / 2;
                days = daysLeft.Days / dosorAmount;
            }
            habitById.DoseAmount = dosorAmount;
            await _habitRepository.ReplaceOneAsync(habitById);
        return habit;

        }
        if(selectedMode == "reduceMode"){
            
        }

        return habit;
    }

    public async Task<HabitDto> GetHabitDtoAsync(string id) 
    {
        var findHabit = await _habitRepository.FindOneAsync(x => x.UserId == id);
        var habitDto = new HabitDto
        {
            DoseType = findHabit.DoseType,
            DoseAmount = findHabit.DoseAmount,
            ProgressionType = findHabit.ProgressionType,
            Speed = int.Parse(findHabit.Speed),
            NumberOfHoursPerDay = findHabit.NumberOfHoursPerDay,
            StartDate = findHabit.StartDate,
            EndDate = findHabit.EndDate,
            WakeUpTime = findHabit.WakeUpTime,
            BedTime = findHabit.BedTime
        };

        return habitDto;
    }

    public async Task<HabitDto> CreateHabitFromRequestAsync(HabitDto newHabit, string userId)
    {

        var habit = new Habit
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            DoseType = newHabit.DoseType,
            DoseAmount = newHabit.DoseAmount,
            ProgressionType = newHabit.ProgressionType,
            Speed = newHabit.Speed.ToString(),
            NumberOfHoursPerDay = newHabit.NumberOfHoursPerDay,
            StartDate = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            EndDate = newHabit.EndDate,
            WakeUpTime = newHabit.WakeUpTime,
            BedTime = newHabit.BedTime
        };
        await _habitRepository.InsertOneAsync(habit);
        var result = await GetHabitDtoAsync(userId);
        await SetRulesForUsersProgression(result, userId);
        result = await GetHabitDtoAsync(userId);
        return result;
        
    }
}