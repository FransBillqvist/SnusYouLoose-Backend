using DAL;

namespace Services.Interfaces;

public interface IProgressionService
{
    Task CreateProgressionAsync(Progression newProgression);
    Task<Progression> GetProgressionAsync(string id);
    Task UpdateProgressionAsync(string id, Progression updatedProgression);
    Task RemoveProgressionAsync(string id);
}