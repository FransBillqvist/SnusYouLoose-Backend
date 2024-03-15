using DAL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]

public class StatisticsController : ControllerBase
{
    private readonly ILogger<StatisticsController> _logger;
    private readonly IGenericMongoRepository<Statistic> _statsRepository;
    private readonly IStatisticsService _statsService;

    public StatisticsController(
        ILogger<StatisticsController> logger,
        IGenericMongoRepository<Statistic> statsRepository,
        IStatisticsService statsService)
        {
            _logger = logger;
            _statsRepository = statsRepository;
            _statsService = statsService;
        }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetStatisticsForPeriod/{userId}/{from}/{to}")]
    public async Task<Statistic> StatisticsForUser(string userId, DateTime from, DateTime to)
    {
        var result = await _statsService.GetStaticsForPeriod(userId, from, to);
        return result;
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetNicotineUsage/{userId}/{period}/")]
    public async Task<NicotineStats> GetNicotineUsage(string userId, string period)
    {
        var result = await _statsService.NicotineUsageOverPeriodCompare(userId, period);
        return result;
    }
    

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("CreateDailyStatistics/{userId}")]
    public async Task<IActionResult> CreateDailyStatistics(string userId)
    {
        await _statsService.CreateDailyStaticsForUser(userId);
        return Ok();
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetLatestStatistics/{userId}")]
    public async Task<Statistic> GetLatestStatistics(string userId)
    {
        var result = await _statsService.GetTemporaryStatisticsOfToday(userId);
        return result;
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("TestRating")]
    public async Task<IActionResult> TestRating(int limit, int used)
    {
        var result = await _statsService.DailyRateStatitics(limit, used);
        return Ok(result);
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("LifeTimeStatistic/{userId}")]

    public async Task<Statistic> LifeTimeStatistic(string userId)
    {
        var result = await _statsService.GetFullUserStatic(userId);
        return result;
    }
}