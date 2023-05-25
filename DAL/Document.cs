using DAL.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

public abstract class Document : IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null;
    [BsonElement("CreatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    internal DateTime CreatedAtUtc { get; set; }
    [BsonElement("StringCreatedAtUtc")]
    public string StringCreatedAtUtc
    {
        get
        {
            return CreatedAtUtc.ToString();
        }
        private set { }
    }

    DateTime IDocument.CreatedAtUtc => throw new NotImplementedException();
}