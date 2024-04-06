namespace DAL;
public class FakeUsageData
{
    public DateOnly StartDate { get; set; }
    public string UserId { get; set; }
    public int DoseAmount { get; set; }
    public int Speed { get; set; }
    public TimeSpan WakeUpTime { get; set; }
    public TimeSpan BedTime { get; set; }
    public double AvgNicotinePerPortion { get; set; }
    public int ProcentOfAllowedOverConsumption { get; set; }
    public int ProcentOfAllowedUnderConsumption { get; set; }
    public int MaxAmountOfBoxes { get; set; }

}