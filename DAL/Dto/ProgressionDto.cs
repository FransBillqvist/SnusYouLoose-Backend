namespace DAL.Dto;

public class ProgressionDto {
    public DateTime GoalStartDate { get; set; }
    public DateTime GoalEndDate { get; set; }
    public int SnuffLimitAmount { get; set; }
    public TimeSpan RecommendedUsageInterval { get; set; }
    public TimeSpan ActualUsageInterval { get; set; }
    public bool InUse { get; set; }
}