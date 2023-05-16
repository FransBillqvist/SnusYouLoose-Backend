using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.Interfaces;

namespace Services;

public class HabitService : IHabitService
{
        private readonly IGenericMongoRepository<Habit> _habitRepository;

        public HabitService(
                IOptions<MongoDbSettings> Settings,
                IGenericMongoRepository<Habit> habitRepository
            )
            {
                _habitRepository = habitRepository;
                var mongoClient = new MongoClient(
                    Settings.Value.ConnectionString);

                var mongoDatabase = mongoClient.GetDatabase(
                        Settings.Value.DatabaseName);
            }

    public async Task<Habit> GetHabitAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        return await _habitRepository.FindOneAsync(x => x.Id == id);
    }

    public async Task CreateHabitAsync(Habit newHabit) =>
        await _habitRepository.InsertOneAsync(newHabit);

    public async Task UpdateHabitAsync(string id, Habit updatedHabit)
    {
        await _habitRepository.ReplaceOneAsync(updatedHabit);
    }

    public async Task RemoveHabitAsync(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        await _habitRepository.DeleteOneAsync(x => x.Id == id);
    }
}