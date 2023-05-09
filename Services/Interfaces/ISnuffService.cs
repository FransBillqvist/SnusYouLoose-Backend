using DAL;

namespace Services.Interfaces;

public interface ISnuffService
{
    Task<Snuff> CreateSnuffAsync(Snuff newSnuff);
    Task<Snuff> GetSnuffAsync(string id);
    Task UpdateSnuffAsync(string id, Snuff updatedSnuff);
    Task RemoveSnuffAsync(string id);
    Task<int> GetSnuffAmountAsync(string snuffId);
}
