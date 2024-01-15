using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Interfaces;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    string Id { get; set; }
    DateTime CreatedAtUtc { get; set; }
    string StringCreatedAtUtc { get; }
}