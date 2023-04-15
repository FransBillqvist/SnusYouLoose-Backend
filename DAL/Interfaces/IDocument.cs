using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Interfaces;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    ObjectId Id {get; set; }
    DateTime CreatedAtUtc {get;}
}