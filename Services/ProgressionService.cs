using DAL;
using DAL.Interfaces;
using Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace Services;

public class ProgressionService : IProgressionService
{
    private readonly IGenericMongoRepository<Progression> _progressionRepository;
    private readonly IGenericMongoRepository<User> _userRepository;
    private readonly IGenericMongoRepository<SnuffLog> _snuffLogRepository;
    private readonly ILogger<ProgressionService> _logger;

    public ProgressionService(
            IOptions<MongoDbSettings> Settings,
            IGenericMongoRepository<Progression> progressionRepository,
            IGenericMongoRepository<User> userRepository,
            IGenericMongoRepository<SnuffLog> snuffLogRepository,
            ILogger<ProgressionService> logger)
    {
        _progressionRepository = progressionRepository;
        _userRepository = userRepository;
        _snuffLogRepository = snuffLogRepository;
        _logger = logger;

        var mongoClient = new MongoClient(
             Settings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
                Settings.Value.DatabaseName);
    }

    public async Task AddNewProgression(Progression newProgression)
    {
        var selectOldProgression = _progressionRepository.FindOneAsync(x => x.UserId == newProgression.UserId);
        if (selectOldProgression == null)
        {
            await _progressionRepository.InsertOneAsync(newProgression);
        }

        var selectProgressionWithInUseTrue = await _progressionRepository.FindOneAsync(x => x.UserId == newProgression.UserId && x.InUse == true);
        if (selectProgressionWithInUseTrue.GoalEndDate > DateTime.Now)
        {
            throw new Exception("User already has an active progression");
        }
        else if (selectProgressionWithInUseTrue.GoalEndDate < DateTime.Now)
        {
            selectProgressionWithInUseTrue.InUse = false;
            await _progressionRepository.ReplaceOneAsync(selectProgressionWithInUseTrue);
        }

        else
        {
            await _progressionRepository.InsertOneAsync(newProgression);
        }
    }

    public Task<int> CalculateRemaingSnuff(string uid)
    {
        throw new NotImplementedException();
    }

    public Task<Progression> FindUserActiveProgression(string uid)
    {
        throw new NotImplementedException();
    }

    public Task RemoveProgressionAsync(string uid)
    {
        throw new NotImplementedException();
    }

    public Task UpdateProgressionStateAsync(Progression updatedProgression)
    {
        throw new NotImplementedException();
    }
}