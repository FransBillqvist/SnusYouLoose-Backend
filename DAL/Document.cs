using DAL.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

public abstract class Document : IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("CreatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("StringCreatedAtUtc")]
    public string StringCreatedAtUtc
    {
        get
        {
            return CreatedAtUtc.ToString();
        }
    }
}