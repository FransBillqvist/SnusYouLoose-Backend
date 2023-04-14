using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Uid { get; set;}
    public string Email {get; set;}
    [BsonElement("Name")]
    public string Name {get; set;}
}