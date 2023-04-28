using DAL;

namespace Services.Interfaces;
public interface ICurrentSnuffService
{
    Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff);
    Task<CurrentSnuff> GetCurrentSnuffAsync(string id);
    Task<CurrentSnuff> LogAdder(string id, CurrentSnuff updatedCurrentSnuff);
}