using DAL.Models;

namespace Services.Interfaces;
public interface IStatisticsService
{
    Task CreateDailyStaticsForUser(string userId);
    Task CreateDailyStaticsForAllUsers();
    Task<Statistic> GetStaticsForPeriod(string userId, DateTime from, DateTime To);

    Task<Statistic> GetTemporaryStatisticsOfToday(string userId);

    Task<double> DailyRateStatitics(int used, int limit);
    Task<Statistic> GetFullUserStatic(string userId);
}