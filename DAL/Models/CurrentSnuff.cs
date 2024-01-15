using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models;

public class CurrentSnuffModel
{
    [BsonId]
    public string CurrentSnuffId { get; set; }
    public string SnusId { get; set; }
    internal DateTime PurchaseDate { get; set; }
    public string StringPurchaseDate
    {
        get
        {
            return PurchaseDate.ToString();
        }
        private set { }
    }
    public int CurrentAmount { get; set; }
}