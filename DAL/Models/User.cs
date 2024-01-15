using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models;

public class UserModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Uid { get; set;}
    public string Email {get; set;}
    public string Name {get; set;}
}