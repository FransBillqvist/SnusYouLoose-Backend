using DAL;

namespace Services.Interfaces;
public interface ISnuffLogService
{
    Task CreateSnuffLogAsync(SnuffLog newSnuffLog);
}