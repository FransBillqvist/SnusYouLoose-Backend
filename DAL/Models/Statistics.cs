using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models;

[BsonCollection("Statistics")]
public class Statistics : Document
{
    public string UserId {get; set;}
    public List<Snuff> UsedSnuffSorts {get; set;} 
    public List<int> UsedAmountOfSnuffs {get; set;}
    public int TotalAmoutUsed {get; set;}
    public int LimitOfUse {get; set;}
    public double Rating {get; set;}
    [BsonElement("CreatedDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedDate { get; set;}
}