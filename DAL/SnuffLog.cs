namespace DAL;

public class SnuffLog
{
    public string SnusLogId {get; set; }
    public string UserId { get; set; }
    public string CurrentSnusId { get; set; }
    public DateTime SnusLogDate { get; set; }
    public int Amount { get; set; }
}