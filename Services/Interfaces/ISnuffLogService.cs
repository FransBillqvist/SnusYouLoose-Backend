using DAL;
using MongoDB.Bson;

namespace Services.Interfaces;
public interface ISnuffLogService
{
    Task<SnuffLog> CreateSnuffLogAsync(SnuffLog newSnuffLog);
    Task<SnuffLog> GetSnuffLogAsync(string id);
    Task UpdateSnuffLogAsync(ObjectId id, SnuffLog updatedSnuffLog);
    Task RemoveSnuffLogAsync(string id);

}