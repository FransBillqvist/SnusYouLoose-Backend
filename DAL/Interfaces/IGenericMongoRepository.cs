using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Interfaces;

public interface IGenericMongoRepository<TDocument> where TDocument : IDocument
{
    IQueryable<TDocument> AsQueryable();
    IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filter);
    IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filter,
        Expression<Func<TDocument, TProjected>> projection);

    TDocument FindOne(Expression<Func<TDocument, bool>> filter);
    Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter);
    TDocument FindById(string id);
    Task<TDocument> FindByIdAsync(string id);
    void InsertOne(TDocument document);
    Task InsertOneAsync(TDocument document);
    void InsertMany(ICollection<TDocument> documents);
    Task InsertManyAsync(ICollection<TDocument> documents);
    void ReplaceOne(TDocument document);
    Task ReplaceOneAsync(TDocument document);
    void DeleteOne(Expression<Func<TDocument, bool>> filter);
    Task DeleteOneAsync(Expression<Func<TDocument, bool>> filter);
    void DeleteById(string id);
    Task DeleteByIdAsync(string id);
    void DeleteMany(Expression<Func<TDocument, bool>> filter);
    Task DeleteManyAsync(Expression<Func<TDocument, bool>> filter);
    IList<TDocument> SearchFor(Expression<Func<TDocument, bool>> predicate);
    Task UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update);
}