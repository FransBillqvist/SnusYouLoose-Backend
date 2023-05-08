using DAL.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

public abstract class Document : IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = null;
    [BsonElement("CreatedAtUtc")]
    public DateTime CreatedAtUtc => DateTime.UtcNow;
}