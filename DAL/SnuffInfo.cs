using System.Reflection.Metadata;
using DAL.Enums;

namespace DAL;
[BsonCollection("SnuffInfo")]
public class SnuffInfo : Document
{
    public string SnusId { get; set; }
    public double NicotinePerGram { get; set; }
    public double NicotinePerPortion { get; set; }
    public List<Flavor> Flavors { get; set; }
    public Format Format { get; set; }

}