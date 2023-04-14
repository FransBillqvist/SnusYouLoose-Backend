using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

public class CurrentSnuff
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonElement("SnusId")]
    public string SnusId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int CurrentAmount { get; set; }
}