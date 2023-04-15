namespace DAL;

[BsonCollection("Snuffs")]
public class Snuff : Document
{
    public string Brand {get; set;}
    public string Type {get; set;}
    public decimal Price { get; set; }
    public int DefaultAmount { get; set; }
}