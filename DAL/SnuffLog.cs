namespace DAL;

[BsonCollection("SnuffLogs")]
public class SnuffLog : Document
{
    public string UserId { get; set; }
    public DateTime SnuffLogDate { get; set; }
    public int AmountUsed { get; set; }
}