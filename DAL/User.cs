using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("Users")]
public class User : Document
{
    [BsonElement("UserId")]
    public string UserId { get; set; }
    [BsonElement("Username")]
    public string Username { get; set; }
    [BsonElement("FirstName")]
    public string FirstName { get; set; }
    [BsonElement("LastName")]
    public string LastName { get; set; }
    [BsonElement("Mobile")]
    public string Mobile { get; set; }
    [BsonElement("BirthDate")]
    public string BirthDate { get; set; }
    [BsonElement("Gender")]
    public string Gender { get; set; }
    [BsonElement("Location")]
    public string Location { get; set; }
    [BsonElement("Password")]
    public string Password { get; set; }
    [BsonElement("Avatar")]
    public string Avatar { get; set; }
    [BsonElement("Email")]
    public string Email { get; set; }
}
