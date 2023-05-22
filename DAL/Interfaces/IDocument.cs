using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Interfaces;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
    internal DateTime CreatedAtUtc { get; }
    public string StringCreateAtUtc
    {
        get
        {
            return CreatedAtUtc.ToString();
        }
        private set { }
    }
}