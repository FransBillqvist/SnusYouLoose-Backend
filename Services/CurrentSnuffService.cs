// using DAL;
// using DAL.Interfaces;
// using DAL.Models;
// using Microsoft.Extensions.Options;
// using MongoDB.Bson;
// using MongoDB.Driver;

// namespace Services;

// public class CurrentSnuffService
// {
//     private readonly IGenericMongoRepository<CurrentSnuff> _currentSnuffRepository;

//     public CurrentSnuffService(
//         IOptions<SnuffDatabaseSettings> snuffDatabaseSettings, IGenericMongoRepository<CurrentSnuff> currentSnuffRepository)
//         {
//             var mongoClient = new MongoClient(
//                 snuffDatabaseSettings.Value.ConnectionString);

//             var mongoDatabase = mongoClient.GetDatabase(
//                     snuffDatabaseSettings.Value.DatabaseName);

//             _currentSnuffRepository = currentSnuffRepository;
//         }

// public async Task<List<CurrentSnuff>> GetAllCurrentSnuffAsync() => 
//     await _currentSnuffRepository.Find(_ => true).ToListAsync();

// public async Task<CurrentSnuff> GetCurrentSnuffAsync(ObjectId id) =>
//     await _currentSnuffRepository.Find(x => x.Id == id).FirstOrDefaultAsync();

// public async Task CreateCurrentSnuffAsync(CurrentSnuff newCurrentSnuff) =>
//     await _currentSnuffRepository.InsertOneAsync(newCurrentSnuff);

// public async Task UpdateCurrentSnuffAsync(ObjectId id, CurrentSnuff updatedCurrentSnuff) =>
//     await _currentSnuffRepository.ReplaceOneAsync(x => x.Id == id, updatedCurrentSnuff);

// public async Task RemoveCurrentSnuffAsync(ObjectId id) =>
//     await _currentSnuffRepository.DeleteOneAsync(x => x.Id == id);
// }