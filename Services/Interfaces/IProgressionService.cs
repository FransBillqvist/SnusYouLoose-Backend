using DAL;

namespace Services.Interfaces;

public interface IProgressionService
{
    Task AddNewProgression(Progression newProgression);
    Task<int> CalculateRemaingSnuff(string uid);
    Task<Progression> FindUserActiveProgression(string uid);
    Task RemoveProgressionAsync(string pid);
    Task UpdateProgressionStateAsync(Progression updatedProgression);
    Task<Progression> ProgressionHandler(Progression progressionData);
}