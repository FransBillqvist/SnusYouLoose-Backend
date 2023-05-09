using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("CurrentSnuffs")]
public class CurrentSnuff : Document
{
    [BsonElement("SnusId")]
    public string SnusId { get; set; }
    public DateTime PurchaseDate { get; set; }
#nullable enable annotations
    public SnuffLog[] LogsOfBox { get; set; }
    public bool IsEmpty { get; set; }
}