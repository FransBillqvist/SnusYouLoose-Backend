using DAL;
using DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Services;

public class HabitService
{
    private readonly IMongoCollection<Habit> _serviceCollection;

    public HabitService(
        IOptions<SnuffDatabaseSettings> snuffDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                snuffDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                    snuffDatabaseSettings.Value.DatabaseName);

            _serviceCollection = mongoDatabase.GetCollection<Habit>(
                    snuffDatabaseSettings.Value.SnuffCollection);
        }
    
    public async Task<List<Habit>> GetAllHabitsAsync() => 
        await _serviceCollection.Find(_ => true).ToListAsync();

    public async Task<Habit> GetHabitAsync(string id) =>
        await _serviceCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateHabitAsync(Habit newHabit) =>
        await _serviceCollection.InsertOneAsync(newHabit);

    public async Task UpdateHabitAsync(string id, Habit updatedHabit) =>
        await _serviceCollection.ReplaceOneAsync(x => x.Id == id, updatedHabit);

    public async Task RemoveHabitAsync(string id) =>
        await _serviceCollection.DeleteOneAsync(x => x.Id == id);
}