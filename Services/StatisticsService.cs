using DAL;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class StatisticsService : IStatisticsService
{
    private readonly IGenericMongoRepository<Statistics> _statisticsRepo;
    private readonly IGenericMongoRepository<Progression> _progressionRepo;
    private readonly IGenericMongoRepository<SnuffLog> _snufflogRepo;
    private readonly IGenericMongoRepository<Snuff> _snuffRepo;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        IOptions<MongoDbSettings> Settings,
        IGenericMongoRepository<Statistics> statisticsRepository,
        IGenericMongoRepository<Progression> progressionRepository,
        IGenericMongoRepository<SnuffLog> snufflogRepository,
        IGenericMongoRepository<Snuff> snuffRepository,
        ILogger<StatisticsService> logger)
        {
            _statisticsRepo = statisticsRepository;
            _progressionRepo = progressionRepository;
            _snufflogRepo = snufflogRepository;
            _snuffRepo = snuffRepository;
            _logger = logger;

            var mongoClient = new MongoClient(
             Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
        }
    public Task CreateDailyStaticsForUser(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Statistics>> GetStaticsForPeriod(string userId, DateTime from, DateTime To)
    {
        throw new NotImplementedException();
    }

    public Task<Statistics> GetTemporaryStatisticsOfToday(string userId)
    {
        throw new NotImplementedException();
    }
}
