namespace DAL.Dto;

public class HabitDto {
    public string DoseType { get; set; }
    public int DoseAmount { get; set; }
    public string ProgressionType { get; set; }
    public int Speed { get; set; }
    public int NumberOfHoursPerDay { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan? WakeUpTime { get; set; }
    public TimeSpan? BedTime { get; set; }

}