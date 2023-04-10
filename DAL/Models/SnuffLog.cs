using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models;

public class SnuffLogModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SnusLogId {get; set; }
    public string UserId { get; set; }
    public string CurrentSnusId { get; set; }
    public DateTime SnusLogDate { get; set; }
    public int Amount { get; set; }
}