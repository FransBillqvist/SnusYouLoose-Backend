using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models;

public class CurrentSnuffModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CurrentSnuffId { get; set; }
    public string SnusId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int CurrentAmount { get; set; }
}