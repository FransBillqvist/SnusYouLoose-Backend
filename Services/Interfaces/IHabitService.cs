using DAL;

namespace Services.Interfaces;
public interface IHabitService
{
    Task CreateHabitAsync(Habit newHabit);
    Task<Habit> GetHabitAsync(string id);
    Task UpdateHabitAsync(string id, Habit updatedHabit);
    Task RemoveHabitAsync(string id);

}