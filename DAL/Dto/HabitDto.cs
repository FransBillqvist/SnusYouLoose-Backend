namespace DAL.Dto;

public class HabitDto {
    public string DoseType { get; set; }
    public int DoseAmount { get; set; }
    public string ProgressionType { get; set; }
    public string Speed { get; set; }
    public int NumberOfHoursPerDay { get; set; } = 17;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

}