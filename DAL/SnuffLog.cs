namespace DAL;

public class SnuffLog
{
    public string SnuffLogId {get; set; }
    public string UserId { get; set; }
    public string CurrentSnusId { get; set; }
    public DateTime SnuffLogDate { get; set; }
    public int Amount { get; set; }
}