using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("SnuffLogs")]
public class SnuffLog : Document
{
    public string UserId { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime SnuffLogDate { get; set; }
    public int AmountUsed { get; set; }
}