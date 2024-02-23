using DAL;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Services.Interfaces;
using System.Linq;

namespace Services;

public class StatisticsService : IStatisticsService
{
    private readonly IGenericMongoRepository<Statistic> _statisticsRepo;
    private readonly IGenericMongoRepository<Progression> _progressionRepo;
    private readonly IGenericMongoRepository<CurrentSnuff> _currentSnuffRepo;
    private readonly IGenericMongoRepository<Snuff> _snuffRepo;
    private readonly IGenericMongoRepository<User> _userRepo;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        IOptions<MongoDbSettings> Settings,
        IGenericMongoRepository<Statistic> statisticsRepository,
        IGenericMongoRepository<Progression> progressionRepository,
        IGenericMongoRepository<CurrentSnuff> snufflogRepository,
        IGenericMongoRepository<Snuff> snuffRepository,
        IGenericMongoRepository<User> userRepository,
        ILogger<StatisticsService> logger)
        {
            _statisticsRepo = statisticsRepository;
            _progressionRepo = progressionRepository;
            _currentSnuffRepo = snufflogRepository;
            _snuffRepo = snuffRepository;
            _userRepo = userRepository;
            _logger = logger;

            var mongoClient = new MongoClient(
             Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
        }
 public async Task CreateDailyStaticsForUser(string userId)
    {
        var user = _userRepo.AsQueryable().Where(x => x.UserId == userId).ToList();

        if (user == null || !user.Any())
        {
            throw new Exception("User not found");
        }

        var accountCreationDate = user.First().CreatedAtUtc.Date;
        var today = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2).Date : DateTime.UtcNow.AddHours(1).Date;
        List<Statistic> statistics = GetUserStatistics(userId);

        var selectTheLatest = statistics.OrderByDescending(statistic => statistic.CreatedAtUtc).FirstOrDefault();
        if (selectTheLatest != null)
        {
            accountCreationDate = selectTheLatest.CreatedDate.Date;
        }
        for (var date = accountCreationDate; date < today; date = date.AddDays(1))
        {
            var i = 0;
            Console.WriteLine("Statistics for the " + date);
            var logList = GetLogList(userId, date);
            List<Progression> progressionList = GetUserProgressions(userId);

            if (progressionList == null)
            {
                Console.WriteLine("NULLLLLLL");
                continue;
            }

            progressionList = progressionList.OrderBy(x => x.GoalStartDate).ToList();
            var rightProgression = progressionList.Find(x => x.GoalStartDate.Date <= date && x.GoalEndDate.Date >= date);
            if (rightProgression == null)
            {
                Console.WriteLine("NULLLLLLL");
                continue;
            }
            var snuffGoalAmount = rightProgression.SnuffGoalAmount;

            int totalUsedSnuff = 0;
            var listOfUsages = new List<int>();
            var usedSnuffSorts = new List<Snuff>();

            foreach (var snuff in logList)
            {
                var logs = snuff.LogsOfBox.Where(log => log.SnuffLogDate.Date == date);
                if (logs != null)
                {
                    var usedSnuff = logs.Sum(log => log.AmountUsed);
                    var snuffObject = _snuffRepo.AsQueryable().FirstOrDefault(s => s.Id == snuff.SnusId);
                    if (usedSnuff >= 1)
                    {
                        listOfUsages.Add(usedSnuff);
                        totalUsedSnuff += usedSnuff;
                        usedSnuffSorts.Add(snuffObject);
                    }
                }
            }

            var newStatistics = new Statistic
            {
                CreatedAtUtc = DateTime.UtcNow,
                UserId = userId,
                UsedSnuffSorts = usedSnuffSorts,
                UsedAmountOfSnuffs = listOfUsages,
                TotalAmoutUsed = totalUsedSnuff,
                LimitOfUse = snuffGoalAmount,
                Rating = await DailyRateStatitics(snuffGoalAmount, totalUsedSnuff),
                CreatedDate = date
            };
            await _statisticsRepo.InsertOneAsync(newStatistics);
            Console.WriteLine("Statistics for the " + date + " Has been saved");
        }

        List<Statistic> GetUserStatistics(string userId)
        {
            return _statisticsRepo.AsQueryable().Where(x => x.UserId == userId).ToList();
        }
    }

    
    public async  Task<Statistic> GetStaticsForPeriod(string userId, DateTime from, DateTime To)
    {
        var allStatistics = _statisticsRepo.AsQueryable().Where(x => x.UserId == userId).ToList();
        var statisticsInPeriod = allStatistics.Where(x => x.CreatedDate.Date >= from.Date && x.CreatedDate.Date <= To.Date).ToList();
        var totalLimit = statisticsInPeriod.Sum(x => x.LimitOfUse);
        var totalUsed = statisticsInPeriod.Sum(x => x.TotalAmoutUsed);
        var totalRatingPoints = statisticsInPeriod.Sum(x => x.Rating);
        var numberOfDays = statisticsInPeriod.Count();
        var finalRating = totalRatingPoints / numberOfDays;

        var (snuffIds, amountOfSnuff) = AggregateSnuffData(statisticsInPeriod.SelectMany(x => x.UsedSnuffSorts).ToList(), statisticsInPeriod.SelectMany(x => x.UsedAmountOfSnuffs).ToList());
        var destictSnuffList = new List<Snuff>();
        foreach (var id in snuffIds)
        {
            var snuff = _snuffRepo.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (snuff != null)
            {
                destictSnuffList.Add(snuff);
            }
        }


        var result = new Statistic
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            UsedSnuffSorts = destictSnuffList,
            UsedAmountOfSnuffs = amountOfSnuff,
            TotalAmoutUsed = statisticsInPeriod.Sum(x => x.TotalAmoutUsed),
            LimitOfUse = statisticsInPeriod.Sum(x => x.LimitOfUse),
            Rating = Math.Round(finalRating, 2, MidpointRounding.AwayFromZero),
            CreatedDate = DateTime.UtcNow.Date,
            NumberOfDays = numberOfDays
        };
        return result;
    }

    public async Task<Statistic> GetTemporaryStatisticsOfToday(string userId)
    {
        var date = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2).Date : DateTime.UtcNow.AddHours(1).Date;
        var progression = GetActiveProgression(userId);
        var logList = GetLogList(userId, date);
        
        var snuffGoalAmount = progression.SnuffGoalAmount;
        int totalUsedSnuff = 0;
        var listOfUsages = new List<int>();
        var usedSnuffSorts = new List<Snuff>();

        foreach (var snuff in logList)
        {
            var logs = snuff.LogsOfBox.Where(log => log.SnuffLogDate.Day == date.Day);

            if (logs != null)
            {
                var usedSnuff = logs.Sum(log => log.AmountUsed);
                var snuffObject = GetSnuffObject(snuff);
                if (usedSnuff >= 1)
                {
                    listOfUsages.Add(usedSnuff);
                    totalUsedSnuff += usedSnuff;
                    usedSnuffSorts.Add(snuffObject);
                }
            }
        }

        var (snuffIds, amountOfSnuff) = AggregateSnuffData(usedSnuffSorts, listOfUsages);
        var destictSnuffList = new List<Snuff>();
        foreach (var id in snuffIds)
        {
            var snuff = _snuffRepo.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (snuff != null)
            {
                destictSnuffList.Add(snuff);
            }
        }

        var newStatistics = new Statistic
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            UsedSnuffSorts = destictSnuffList,
            UsedAmountOfSnuffs = amountOfSnuff,
            TotalAmoutUsed = totalUsedSnuff,
            LimitOfUse = snuffGoalAmount,
            Rating = await DailyRateStatitics(snuffGoalAmount, totalUsedSnuff),
            CreatedDate = DateTime.UtcNow.Date,
            Id = Guid.NewGuid().ToString(),
            NumberOfDays = 0
        };
        return await Task.FromResult(newStatistics);
    }


    public async Task<double> DailyRateStatitics(int limit, int used)
    {
    var theValueOfOneSnuff = 100 / limit;
    var diff = limit - used;
    double rating;

    if (used == limit)
    {
        rating = 100.00;
    }
    else if(used == 0 && limit != 0)
    {
        rating = 250.00;
    }
    else if (used < limit)
    {
        rating = 100 + (limit - used) * theValueOfOneSnuff;
    }
    else // totalUsedAmount > totalAmountAllowed
    {
        if(limit * 2 < used)
        {
            rating = 0;
        }
        else
        {
            rating = 100 - ((used - limit) * theValueOfOneSnuff);
        }
    }

    return await Task.FromResult(rating);
    }

    public async Task<Statistic> GetFullUserStatic(string userId)
    {
        var allStatistics = _statisticsRepo.AsQueryable().Where(x => x.UserId == userId).ToList();
        var todaysStatistic = await GetTemporaryStatisticsOfToday(userId);
        var firstDate = allStatistics.OrderBy(x => x.CreatedDate).FirstOrDefault();
        var lastDate = allStatistics.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        allStatistics.Add(todaysStatistic);
        var totalLimit = allStatistics.Sum(x => x.LimitOfUse);
        var totalUsed = allStatistics.Sum(x => x.TotalAmoutUsed);
        var totalRatingPoints = allStatistics.Sum(x => x.Rating);
        var numberOfDays = allStatistics.Count();
        var finalRating = totalRatingPoints / numberOfDays;
        var statisticsForDays = (lastDate.CreatedDate - firstDate.CreatedDate).Days;


        var (snuffIds, amountOfSnuff) = AggregateSnuffData(allStatistics.SelectMany(x => x.UsedSnuffSorts).ToList(), allStatistics.SelectMany(x => x.UsedAmountOfSnuffs).ToList());
        var destictSnuffList = new List<Snuff>();
        foreach (var id in snuffIds)
        {
            var snuff = _snuffRepo.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (snuff != null)
            {
                destictSnuffList.Add(snuff);
            }
        }

        var result = new Statistic
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            UsedSnuffSorts = destictSnuffList,
            UsedAmountOfSnuffs = amountOfSnuff,
            TotalAmoutUsed = allStatistics.Sum(x => x.TotalAmoutUsed),
            LimitOfUse = allStatistics.Sum(x => x.LimitOfUse),
            Rating = Math.Round(finalRating, 2, MidpointRounding.AwayFromZero),
            CreatedDate = DateTime.UtcNow.Date,
            NumberOfDays = statisticsForDays
        };
        return result;
    }

    private (List<string> SnuffIds, List<int> TotalAmounts) AggregateSnuffData(List<Snuff> snuffs, List<int> amounts)
    {
        var combined = snuffs.Zip(amounts, (s, a) => new { Snuff = s, Amount = a });

        var aggregatedData = combined
            .GroupBy(x => x.Snuff.Id)
            .Select(g => new
            {
                SnuffId = g.Key,
                TotalAmount = g.Sum(x => x.Amount)
            })
            .ToList();

        // Separera den aggregerade datan till två listor
        List<string> snuffIds = aggregatedData.Select(x => x.SnuffId).ToList();
        List<int> totalAmounts = aggregatedData.Select(x => x.TotalAmount).ToList();

        return (snuffIds, totalAmounts);
    }
    private List<Progression> GetUserProgressions(string userId)
    {
        return _progressionRepo.AsQueryable().Where(x => x.UserId == userId).ToList();
    }

    private static IEnumerable<object> GetUsedSnuffList(DateTime date, IEnumerable<CurrentSnuff> ListOfAllLogs)
    {
        return  ListOfAllLogs.GroupBy(x => x.SnusId).Select(x => new { SnusId = x.Key, Amount = x.Sum(y => y.LogsOfBox.Where(z => z.CreatedAtUtc.Date == date).Sum(z => z.AmountUsed)) });
    }
    private Snuff GetSnuffObject(CurrentSnuff snuff)
    {
        var result =_snuffRepo.AsQueryable().FirstOrDefault(s => s.Id == snuff.SnusId);
        if(result == null)
        {
            throw new Exception("No snuff found");
        }
        return result;
    }

    private  Progression GetActiveProgression(string userId)
    {
        var result = _progressionRepo.SearchFor(x => x.UserId == userId && x.InUse == true).FirstOrDefault();
        if(result == null)
        {
            throw new Exception("No progression found");
        }
        return result;
    }

    private  IEnumerable<CurrentSnuff> GetLogList(string userId, DateTime date)
    {
        var getAllCurrentSnuff = _currentSnuffRepo.AsQueryable().Where(x => x.UserId == userId).ToList();
        if(getAllCurrentSnuff == null)
        {
            throw new Exception("No logs found");
        }
        var result = getAllCurrentSnuff.Where(x => x.LogsOfBox.All(log => log.SnuffLogDate.Day == date.Day));

        //V1
        // var result = _currentSnuffRepo.SearchFor(x => x.UserId == userId && x.LogsOfBox.All(log => log.SnuffLogDate.Date == date));
        // foreach (var item in result)
        // {
        //     Console.WriteLine("SnuffId: " + item.LogsOfBox.Select(x => x.SnuffLogDate.Date).FirstOrDefault());
        // }
        return result;
    }
}
