using System.Linq.Expressions;
using DAL;
using DAL.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository;

public class MongoRepository<TDocument> : IGenericMongoRepository<TDocument> where TDocument : IDocument
{
    private readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(MongoDbSettings settings)
    {
        var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
    }

    private protected string GetCollectionName(Type documentType)
    {
        var collectionName = ((BsonCollectionAttribute)documentType.GetCustomAttributes(
            typeof(BsonCollectionAttribute),
            true)
            .FirstOrDefault()).CollectionName;

        if (collectionName != null)
        {
            return collectionName.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    public virtual IQueryable<TDocument> AsQueryable()
    {
        return _collection.AsQueryable();
    }

    public virtual IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filter)
    {
        return _collection.Find(filter).ToEnumerable();
    }

    public virtual IEnumerable<TProjected> FilterBy<TProjected>(Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TProjected>> projection)
    {
        return (IEnumerable<TProjected>)Task.Run(() => _collection.Find(filter).Project(projection).ToEnumerable());
    }

    public virtual TDocument FindById(string id)
    {
        ObjectId objectId;
        ObjectId.TryParse(id, out objectId);
        var filter = Builders<TDocument>.Filter.Eq(id, objectId);
        return _collection.Find(filter).SingleOrDefault();
    }

    public virtual Task<TDocument> FindByIdAsync(string id)
    {
        return Task.Run(() =>
        {
            ObjectId objectId;
            ObjectId.TryParse(id, out objectId);
            var filter = Builders<TDocument>.Filter.Eq(id, objectId);
            return _collection.Find(filter).SingleOrDefaultAsync();
        });
    }

    public virtual TDocument FindOne(Expression<Func<TDocument, bool>> filter)
    {
        return _collection.Find(filter).FirstOrDefault();
    }

    public virtual Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter)
    {
        return Task.Run(() => _collection.Find(filter).FirstOrDefaultAsync());
    }

    public virtual void InsertOne(TDocument document)
    {
        _collection.InsertOne(document);
    }

    public Task InsertOneAsync(TDocument document)
    {
        return Task.Run(() => _collection.InsertOneAsync(document));
    }
    public void InsertMany(ICollection<TDocument> documents)
    {
        _collection.InsertMany(documents);
    }

    public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
    {
        await _collection.InsertManyAsync(documents);
    }


    public void ReplaceOne(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        _collection.FindOneAndReplace(filter, document);
    }

    public virtual async Task ReplaceOneAsync(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await _collection.FindOneAndReplaceAsync(filter, document);
    }

    public void DeleteOne(Expression<Func<TDocument, bool>> filter)
    {
        _collection.FindOneAndDelete(filter);
    }

    public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filter)
    {
        return Task.Run(() => _collection.FindOneAndDelete(filter));
    }

    public void DeleteById(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<TDocument>.Filter.Eq(id, objectId.ToString);
        _collection.FindOneAndDelete(filter);
    }

    public Task DeleteByIdAsync(string id)
    {
        return Task.Run(() =>
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(id, objectId.ToString);
            _collection.FindOneAndDeleteAsync(filter);
        });
    }

    public void DeleteMany(Expression<Func<TDocument, bool>> filter)
    {
        _collection.DeleteMany(filter);
    }

    public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filter)
    {
        return Task.Run(() => _collection.DeleteManyAsync(filter));
    }

    public IList<TDocument> SearchFor(Expression<Func<TDocument, bool>> predicate) => _collection
           .AsQueryable<TDocument>()
           .Where(predicate.Compile())
           .ToList();
    public async Task UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
    {
        await _collection.UpdateOneAsync(filter, update);
    }
}