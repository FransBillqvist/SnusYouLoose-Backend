using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

public class SnuffLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get; set; }
    public string UserId { get; set; }
    public string CurrentSnusId { get; set; }
    public DateTime SnuffLogDate { get; set; }
    public int Amount { get; set; }
}