using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models;

public class SnuffModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get; set;}
    public string Brand {get; set;}
    public string Type {get; set;}
    public decimal Price { get; set; }
    public int DefaultAmount { get; set; }
}