using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("Progression")]
public class Progression : Document
{
    [BsonElement("UserId")]
    public string UserId { get; set; }
    [BsonElement("GoalStartDate")]
    public DateTime GoalStartDate { get; set; }
    [BsonElement("GoalEndDate")]
    public DateTime GoalEndDate { get; set; }
    [BsonElement("SnuffGoalAmount")]
    public int SnuffGoalAmount { get; set; }
    [BsonElement("UsageInterval")]
    public DateTime UsageInterval { get; set; }
    [BsonElement("InUse")]
    public Boolean InUse { get; set; }
}