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
    private readonly IGenericMongoRepository<Statistics> _statsRepository;
    private readonly IStatisticsService _statsService;

    public StatisticsController(
        ILogger<StatisticsController> logger,
        IGenericMongoRepository<Statistics> statsRepository,
        IStatisticsService statsService)
        {
            _logger = logger;
            _statsRepository = statsRepository;
            _statsService = statsService;
        }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetStatisticsForPeriod/{userId}/{from}/{to}")]
    public async Task<List<Statistics>> StatisticsForUser(string userId, DateTime from, DateTime to)
    {
        var EmptyStats = new Statistics();
        var result = new List<Statistics> { EmptyStats };
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
    public async Task<Statistics> GetLatestStatistics(string userId)
    {
        var result = await _statsService.GetTemporaryStatisticsOfToday(userId);
        return result;
    }
}