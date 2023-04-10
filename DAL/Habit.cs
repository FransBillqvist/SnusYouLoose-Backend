namespace DAL;

public class Habit
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string DoseType { get; set; }
    public int DoseAmount { get; set; }
    public string ProgressionType {get; set; }
    public DateTime EndDate {get; set; }
    public DateTime StartDate {get; set; }
}