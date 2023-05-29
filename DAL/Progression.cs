using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("Progression")]
public class Progression : Document
{
    [BsonElement("UserId")]
    public string UserId { get; set; }
    [BsonElement("GoalStartDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
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
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
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
    [BsonElement("RecommendedUsageInterval")]
    public TimeSpan RecommendedUsageInterval { get; set; }
    [BsonElement("ActualUsageInterval")]
    public string ActualUsageInterval { get; set; } = string.Empty;
    [BsonElement("InUse")]
    public Boolean InUse { get; set; }
}