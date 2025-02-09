using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL;

[BsonCollection("CurrentSnuffs")]
public class CurrentSnuff : Document
{
    [BsonElement("SnusId")]
    public string SnusId { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    internal DateTime PurchaseDate { get; set; }
    public string StringPurchaseDate
    {
        get
        {
            return PurchaseDate.ToString();
        }
        private set { }
    }
    public SnuffLog[] LogsOfBox { get; set; } = Array.Empty<SnuffLog>();
    public string UserId { get; set; }
    public bool IsEmpty { get; set; }
    public bool IsArchived { get; set; }
    public int RemainingAmount { get; set; }
}