using DAL.Models;

namespace Services.Interfaces;
public interface IStatisticsService
{
    Task CreateDailyStaticsForUser(string userId);
    Task<List<Statistics>> GetStaticsForPeriod(string userId, DateTime from, DateTime To);
}