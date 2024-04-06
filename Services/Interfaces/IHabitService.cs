using DAL;
using DAL.Dto;

namespace Services.Interfaces;
public interface IHabitService
{
    Task CreateHabitAsync(Habit newHabit);
    Task<Habit> GetHabitAsync(string id);
    Task UpdateHabitAsync(string id, Habit updatedHabit);
    Task RemoveHabitAsync(string id);
    Task<HabitDto> GetHabitDtoAsync(string id);
    Task<HabitDto> CreateHabitFromRequestAsync(HabitDto newHabit, string userId);
    Task<HabitDto> SetRulesForUsersProgression(HabitDto habit,  string userId);

}