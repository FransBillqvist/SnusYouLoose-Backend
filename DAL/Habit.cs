namespace DAL;

[BsonCollection("Habits")]
public class Habit : Document
{
    public string UserId { get; set; }
    public string DoseType { get; set; }
    public int DoseAmount { get; set; }
    public string ProgressionType { get; set; }
    public string Speed { get; set; }
    internal DateTime EndDate { get; set; }
    internal DateTime StartDate { get; set; } = DateTime.Now;
    public string StringEndDate
    {
        get
        {
            return EndDate.ToString();
        }
        set { StringEndDate = EndDate.ToString(); }
    }
    public string StringStartDate { get; set; } = string.Empty;
}