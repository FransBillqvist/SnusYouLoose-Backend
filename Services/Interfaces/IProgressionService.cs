using DAL;

namespace Services.Interfaces;

public interface IProgressionService
{
    Task<Progression> AddNewProgression(string uid);
    Task<int> CalculateRemainingSnuff(string uid);
    Task<Progression> FindUserActiveProgression(string uid);
    Task RemoveProgressionAsync(string pid);
    Task<Progression> UpdateProgressionStateAsync(Progression updatedProgression);
    Task<Progression> ProgressionHandler(string uid);
    Task<TimeSpan> WhenIsTheNextDoseAvailable(string uid);
    Task<TimeSpan> LastConsumedSnuffAtUtc(string uid);
}