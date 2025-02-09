using DAL;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Services.Interfaces;
using System.Linq;
using System.Text.RegularExpressions;

namespace Services;

public class StatisticsService : IStatisticsService
{
    private readonly IGenericMongoRepository<Statistic> _statisticsRepo;
    private readonly IGenericMongoRepository<Progression> _progressionRepo;
    private readonly IGenericMongoRepository<CurrentSnuff> _currentSnuffRepo;
    private readonly IGenericMongoRepository<Snuff> _snuffRepo;
    private readonly IGenericMongoRepository<SnuffInfo> _snuffInfoRepo;
    private readonly IGenericMongoRepository<SnuffLog> _snufflogRepo;
    private readonly IGenericMongoRepository<User> _userRepo;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        IOptions<MongoDbSettings> Settings,
        IGenericMongoRepository<Statistic> statisticsRepository,
        IGenericMongoRepository<Progression> progressionRepository,
        IGenericMongoRepository<CurrentSnuff> currentRepository,
        IGenericMongoRepository<SnuffLog> snufflogRepository,
        IGenericMongoRepository<Snuff> snuffRepository,
        IGenericMongoRepository<SnuffInfo> snuffInfoRepository,
        IGenericMongoRepository<User> userRepository,
        ILogger<StatisticsService> logger)
        {
            _statisticsRepo = statisticsRepository;
            _progressionRepo = progressionRepository;
            _currentSnuffRepo = currentRepository;
            _snufflogRepo = snufflogRepository;
            _snuffRepo = snuffRepository;
            _snuffInfoRepo = snuffInfoRepository;
            _userRepo = userRepository;
            _logger = logger;

            var mongoClient = new MongoClient(
             Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
        }


    public async Task CreateDailyStaticsForAllUsers()
    {
        var allUsers = _userRepo.AsQueryable().ToList();
        foreach (var user in allUsers)
        {
            Console.WriteLine("Creating statistics for " + user.UserId);
            await CreateDailyStaticsForUser(user.UserId);
        }
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


            var (usedSnuffSorts, listOfUsages, totalUsedSnuff) = GetSnuffAmountAndTotaltUsage(logList.ToList(), date);
            var (snuffIds, amountOfSnuff) = AggregateSnuffData(usedSnuffSorts, listOfUsages);
            var costOfPurschase = Math.Round(await CalcualtePurschaseCost(logList.ToList(), date), 2, MidpointRounding.AwayFromZero);
            var costOfDailyUsage = Math.Round(await SnuffUsageCost(snuffIds, amountOfSnuff), 2, MidpointRounding.AwayFromZero);

            var newStatistics = new Statistic
            {
                CreatedAtUtc = DateTime.UtcNow,
                UserId = userId,
                UsedSnuffSorts = usedSnuffSorts,
                UsedAmountOfSnuffs = listOfUsages,
                TotalAmoutUsed = totalUsedSnuff,
                LimitOfUse = snuffGoalAmount,
                Rating = await DailyRateStatitics(snuffGoalAmount, totalUsedSnuff),
                PurchaseCost = costOfPurschase,
                UsageCost = costOfDailyUsage,
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
        var numberOfDays = statisticsInPeriod.Select(x => x.CreatedDate.Date).Distinct().Count();
        var finalRating = totalRatingPoints / numberOfDays;
        var costOfAllSnuffBought = statisticsInPeriod.Sum(x => x.PurchaseCost);
        var totalCostOfUsage = statisticsInPeriod.Sum(x => x.UsageCost);
        var (snuffIds, amountOfSnuff) = AggregateSnuffData(statisticsInPeriod.SelectMany(x => x.UsedSnuffSorts).ToList(), statisticsInPeriod.SelectMany(x => x.UsedAmountOfSnuffs).ToList());
        var destictSnuffList = await GetCompletedSnuffList(snuffIds);

        var result = new Statistic
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            UsedSnuffSorts = destictSnuffList,
            UsedAmountOfSnuffs = amountOfSnuff,
            TotalAmoutUsed = statisticsInPeriod.Sum(x => x.TotalAmoutUsed),
            LimitOfUse = statisticsInPeriod.Sum(x => x.LimitOfUse),
            Rating = Math.Round(finalRating, 2, MidpointRounding.AwayFromZero),
            PurchaseCost = Math.Round(costOfAllSnuffBought, 2, MidpointRounding.AwayFromZero),
            UsageCost = Math.Round(totalCostOfUsage, 2, MidpointRounding.AwayFromZero),
            CreatedDate = DateTime.UtcNow.Date,
            NumberOfDays = numberOfDays
        };
        return await Task.FromResult(result);
    }

    public async Task<Statistic> GetTemporaryStatisticsOfToday(string userId)
    {
        var date = DateTime.Now;
        var progression = GetActiveProgression(userId);
        var logList = GetLogList(userId, date);
        var snuffGoalAmount = progression.SnuffGoalAmount;
        var (usedSnuffSorts, listOfUsages, totalUsedSnuff) = GetSnuffAmountAndTotaltUsage(logList.ToList(), date);
        var (snuffIds, amountOfSnuff) = AggregateSnuffData(usedSnuffSorts, listOfUsages);
        var destictSnuffList = await GetCompletedSnuffList(snuffIds);

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
            PurchaseCost = Math.Round(await CalcualtePurschaseCost(logList.ToList(), date), 2, MidpointRounding.AwayFromZero),
            UsageCost = Math.Round(await SnuffUsageCost(snuffIds, amountOfSnuff), 2, MidpointRounding.AwayFromZero),
            Id = "Temporay",
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
        var totalPurchaseCost = allStatistics.Sum(x => x.PurchaseCost);
        var totalUsageCost = allStatistics.Sum(x => x.UsageCost);


        var (snuffIds, amountOfSnuff) = AggregateSnuffData(allStatistics.SelectMany(x => x.UsedSnuffSorts).ToList(), allStatistics.SelectMany(x => x.UsedAmountOfSnuffs).ToList());
        var destictSnuffList = await GetCompletedSnuffList(snuffIds);

        var result = new Statistic
        {
            CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1),
            UserId = userId,
            UsedSnuffSorts = destictSnuffList,
            UsedAmountOfSnuffs = amountOfSnuff,
            TotalAmoutUsed = allStatistics.Sum(x => x.TotalAmoutUsed),
            LimitOfUse = allStatistics.Sum(x => x.LimitOfUse),
            Rating = Math.Round(finalRating, 2, MidpointRounding.AwayFromZero),
            PurchaseCost = Math.Round(totalPurchaseCost, 2, MidpointRounding.AwayFromZero),
            UsageCost = Math.Round(totalUsageCost, 2, MidpointRounding.AwayFromZero),
            CreatedDate = DateTime.UtcNow.Date,
            NumberOfDays = statisticsForDays+1
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
    private Snuff GetASnuffObject(CurrentSnuff snuff)
    {
        var result = _snuffRepo.AsQueryable().FirstOrDefault(s => s.Id == snuff.SnusId);
        if(result == null)
        {
            throw new Exception("No snuff found");
        }

        result.SnuffInfo = _snuffInfoRepo.AsQueryable().FirstOrDefault(s => s.SnusId == snuff.SnusId) ?? new SnuffInfo();
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
        var result = getAllCurrentSnuff.Where(x => x.LogsOfBox.All(x => true)).ToList();
        return result;
    }

    private async Task<decimal> CalcualtePurschaseCost(List<CurrentSnuff> snuffList, DateTime date)
    {
        decimal result = 0m;
        var boughtToday = snuffList.Where(x => x.PurchaseDate.Day == date.Day && x.PurchaseDate.Month == date.Month && x.PurchaseDate.Year == date.Year).ToList();
        if(boughtToday != null)
        {
            foreach(var box in boughtToday)
            {
                var snuffBox =  _snuffRepo.AsQueryable().Where(x => x.Id == box.SnusId).FirstOrDefault();
                if(snuffBox != null)
                {  
                    result += snuffBox.Price;
                }
            }
        }
        return await Task.FromResult(result);
    }

    private async Task<decimal> SnuffUsageCost(List<string> ids, List<int> amount)
    {
        decimal result = 0.0m;
        var length = amount.Count;
        var snuffList = new List<Snuff>();
        foreach(var id in ids)
        {
            var findSnuff = _snuffRepo.AsQueryable().Where(x => x.Id == id).FirstOrDefault();
            if(findSnuff != null)
            {
                snuffList.Add(findSnuff);
            }
        }
        for(var i = 0; i < length; i++)
        {
            var boxPrice = snuffList[i].Price;
            var doses = snuffList[i].DefaultAmount;
            var priceOfOneDose = boxPrice/doses;
            result += priceOfOneDose * amount[i];
        }
        return await Task.FromResult(result);
    }
    private (List<Snuff> snuffList, List<int> amountList, int totaltamount) GetSnuffAmountAndTotaltUsage(List<CurrentSnuff> logList, DateTime date)
    {
        int totalUsedSnuff = 0;
        var listOfUsages = new List<int>();
        var usedSnuffSorts = new List<Snuff>();
        foreach (var snuff in logList)
            {
                var logs = snuff.LogsOfBox.Where(log => log.SnuffLogDate.Day == date.Day && log.SnuffLogDate.Month == date.Month && log.SnuffLogDate.Year == date.Year);
                if (logs != null)
                {
                    var usedSnuff = logs.Sum(log => log.AmountUsed);
                    var snuffObject = GetASnuffObject(snuff);
                    if (usedSnuff >= 1)
                    {
                        listOfUsages.Add(usedSnuff);
                        totalUsedSnuff += usedSnuff;
                        if(snuffObject != null)
                        {
                        usedSnuffSorts.Add(snuffObject);
                        }
                    }
                }
            }
        return (usedSnuffSorts, listOfUsages, totalUsedSnuff);
    }
    private async Task<List<Snuff>> GetCompletedSnuffList(List<string> ids)
    {
        var destictSnuffList = new List<Snuff>();
        foreach (var id in ids)
        {
            var snuff = _snuffRepo.AsQueryable().FirstOrDefault(x => x.Id == id);
            var snuffInfo = _snuffInfoRepo.AsQueryable().FirstOrDefault(x => x.SnusId == id);
            if (snuff != null)
            {
                if(snuffInfo != null)
                {
                    snuff.SnuffInfo = snuffInfo;
                }
                destictSnuffList.Add(snuff);
            }
        }
        return await Task.FromResult(destictSnuffList);
    }

    

    public async Task<NicotineStats> NicotineUsageOverPeriodCompare(string userId, string period)
    {
       switch(period)
        {
            case "today":
                return await CreateNicotineUsageStats(userId, DateTime.Now.Date.AddDays(-1), DateTime.Now.Date);
            case "week":
                return await CreateNicotineUsageStats(userId, DateTime.Now.Date.AddDays(-8), DateTime.Now.Date.AddDays(-1));
            case "twoweek":
                return await CreateNicotineUsageStats(userId, DateTime.Now.Date.AddDays(-15), DateTime.Now.Date.AddDays(-1));
            case "month":
                return await CreateNicotineUsageStats(userId, DateTime.Now.Date.AddDays(-31), DateTime.Now.Date.AddDays(-1));
            case "quarter":
                return await CreateNicotineUsageStats(userId, DateTime.Now.Date.AddDays(-91), DateTime.Now.Date.AddDays(-1));
            case "year":
                return await CreateNicotineUsageStats(userId, DateTime.Now.Date.AddDays(-366), DateTime.Now.Date.AddDays(-1));
            default:
                throw new ArgumentException("Invalid period value.");
        }
    }

    private async Task<NicotineStats> CreateNicotineUsageStats(string userId, DateTime from, DateTime to)
    {

        var lengthOfPeriod = (to - from).Days;
        var secondaryFrom = from.AddDays(-lengthOfPeriod-1);
        var secondaryto = from.AddDays(-1);

        var result = new NicotineStats();
        
        var allOfUserStatistics = _statisticsRepo.AsQueryable().Where(x => x.UserId == userId).ToList();
        var statisticsInPrimaryPeriod = allOfUserStatistics.Where(x => x.CreatedDate.Date >= from.Date && x.CreatedDate.Date <= to.Date).ToList();
        var (primarySnuffIds, primaryAmountList) = AggregateSnuffData(statisticsInPrimaryPeriod.SelectMany(x => x.UsedSnuffSorts).ToList(), statisticsInPrimaryPeriod.SelectMany(x => x.UsedAmountOfSnuffs).ToList());
        var snuffList = await GetCompletedSnuffList(primarySnuffIds);

        
        var (UsageInMgPrimary, primaryDays) = CalculateNicotineUsage(snuffList, primaryAmountList);
        result.PrimaryNicotineUse = UsageInMgPrimary;
        result.PrimaryNumberOfDays = primaryDays;

        var statisticsInSecondaryPeriod = allOfUserStatistics.Where(x => x.CreatedDate.Date >= secondaryFrom.Date && x.CreatedDate.Date <= secondaryto.Date).ToList();
        var (secondarySnuffIds, secondaryAmountList) = AggregateSnuffData(statisticsInSecondaryPeriod.SelectMany(x => x.UsedSnuffSorts).ToList(), statisticsInSecondaryPeriod.SelectMany(x => x.UsedAmountOfSnuffs).ToList());
        var secondarySnuffList = await GetCompletedSnuffList(secondarySnuffIds);

        var (UsageInMgSecoundary, secoundaryDays) = CalculateNicotineUsage(secondarySnuffList, secondaryAmountList);
        result.SecondaryNicotineUse = UsageInMgSecoundary;
        result.SecondaryNumberOfDays = secoundaryDays;
        
        result.IndicationScore = CalculateIndicationScore(result.PrimaryNicotineUse, result.SecondaryNicotineUse);

        return await Task.FromResult(result); 
    }

    private decimal CalculateIndicationScore(decimal primary, decimal secondary)
    {
        return secondary / primary;
    }

    private (decimal, int) CalculateNicotineUsage(List<Snuff> snuffs, List<int> amounts)
    {
        decimal UsageInMg = 0.0m;
        var days = 0;
        var length = amounts.Count;
        for (var i = 0; i < length; i++)
        {
            var nicotine = snuffs[i].SnuffInfo!.NicotinePerPortion;
            UsageInMg+= (decimal)nicotine * amounts[i];
            days++;
        }
        return (UsageInMg, days);
    }

}
