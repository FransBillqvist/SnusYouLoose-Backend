using DAL;

namespace Services.Interfaces;

public interface IProgressionService
{
    Task AddNewProgression(Progression newProgression);
    Task<Progression> FindUserActiveProgression(string uid);
    Task UpdateProgressionStateAsync(Progression updatedProgression);
    Task RemoveProgressionAsync(string uid);
    Task<int> CalculateRemaingSnuff(string uid);
}