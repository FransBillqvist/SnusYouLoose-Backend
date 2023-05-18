using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("Users")]
public class User : Document
{
    [BsonElement("Email")]
    public string Email { get; set; }
    [BsonElement("Name")]
    public string Name { get; set; }
    [BsonElement("Password")]
    public string Password { get; set; }
    
}