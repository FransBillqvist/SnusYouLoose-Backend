namespace DAL.Models;

public class SnuffDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string SnuffCollection { get; set; } = null!;
}