using DAL;

namespace Services.Interfaces;
public interface ICurrentSnuffService
{
    Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff);
    Task<CurrentSnuff> GetCurrentSnuffAsync(string id);
    Task<CurrentSnuff> LogAdder(string id, int amount, string userId);
    Task RemoveCurrentSnuffAsync(string id);
    Task UpdateCurrentSnuffAsync(string id, CurrentSnuff updatedCurrentSnuff);
}