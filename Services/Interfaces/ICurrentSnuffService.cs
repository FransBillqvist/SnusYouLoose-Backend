using DAL;
using DAL.Dto;

namespace Services.Interfaces;
public interface ICurrentSnuffService
{
    Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff);
    Task<CurrentSnuff> CreateCurrentSnuffWithDtoAsync(CreateCSDto newCurrentSnuff);
    Task<CurrentSnuff> GetCurrentSnuffAsync(string id);
    Task<CurrentSnuff> LogAdder(string id, int amount, string userId);
    Task<CurrentSnuff> LogAdderV2(string csId, int amount);
    Task RemoveCurrentSnuffAsync(string id);
    Task UpdateCurrentSnuffAsync(string id, CurrentSnuff updatedCurrentSnuff);
    Task<List<CurrentSnuff>> GetAllCurrentSnuffsForThisUserAsync(string uid);
    Task<Boolean> AddCurrentSnuffToArchiveAsync(string CurrentSnuffId);
    Task<int> GetAmountInBoxAsync(string csid);
    Task<List<CurrentSnuffDto>> GetCurrentSnuffInventoryAsync(string userId);
}