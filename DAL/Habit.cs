namespace DAL;

[BsonCollection("Habits")]
public class Habit : Document
{
    public string UserId { get; set; }
    public string DoseType { get; set; }
    public int DoseAmount { get; set; }
    public string ProgressionType { get; set; }
    public string Speed { get; set; }
    public int NumberOfHoursPerDay { get; set; } = 17;
    internal DateTime EndDate { get; set; }
    internal DateTime StartDate { get; set; }
    public string StringEndDate
    {
        get
        {
            return EndDate.ToString();
        }
        private set { }
    }
    public string StringStartDate
    {
        get
        {
            return StartDate.ToString();
        }
        private set { }
    }
}