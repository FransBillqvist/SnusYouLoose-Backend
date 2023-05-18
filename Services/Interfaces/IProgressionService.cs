using DAL;

namespace Services.Interfaces;

public interface IProgressionService
{
    Task AddNewProgression(string uid);
    Task<int> CalculateRemainingSnuff(string uid);
    Task<Progression> FindUserActiveProgression(string uid);
    Task RemoveProgressionAsync(string pid);
    Task UpdateProgressionStateAsync(Progression updatedProgression);
    Task<Progression> ProgressionHandler(string uid);
}