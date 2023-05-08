using DAL;

namespace Services.Interfaces;
public interface ISnuffLogService
{
    Task<SnuffLog> CreateSnuffLogAsync(SnuffLog newSnuffLog);
}