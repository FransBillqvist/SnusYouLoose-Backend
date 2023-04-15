namespace DAL;

[BsonCollection("SnuffLogs")]
public class SnuffLog : Document
{
    public string UserId { get; set; }
    public string CurrentSnusId { get; set; }
    public DateTime SnuffLogDate { get; set; }
    public int Amount { get; set; }
}