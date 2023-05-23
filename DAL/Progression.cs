using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("Progression")]
public class Progression : Document
{
    [BsonElement("UserId")]
    public string UserId { get; set; }
    [BsonElement("GoalStartDate")]
    internal DateTime GoalStartDate { get; set; }

    [BsonElement("StringGoalStartDate")]
    public string StringGoalStartDate
    {
        get
        {
            return GoalStartDate.ToString();
        }
        private set { }
    }
    [BsonElement("GoalEndDate")]
    internal DateTime GoalEndDate { get; set; }

    [BsonElement("StringGoalEndDate")]
    public string StringGoalEndDate
    {
        get
        {
            return GoalEndDate.ToString();
        }
        private set { }
    }
    [BsonElement("SnuffGoalAmount")]
    public int SnuffGoalAmount { get; set; }
    [BsonElement("UsageInterval")]
    public TimeSpan RecommendedUsageInterval { get; set; }
    public TimeSpan ActualUsageInterval { get; set; }
    [BsonElement("InUse")]
    public Boolean InUse { get; set; }
}