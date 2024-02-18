using System.Reflection.Metadata;
using DAL.Enums;

namespace DAL;
public class SnuffInfo : Document
{
    public string SnusId { get; set; }
    public double NicotinePerGram { get; set; }
    public double NicotinePerPortion { get; set; }
    public List<Flavor> Flavors { get; set; }
    public Fromat Format { get; set; }

}