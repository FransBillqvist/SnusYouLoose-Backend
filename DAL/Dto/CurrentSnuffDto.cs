namespace DAL.Dto;
public class CurrentSnuffDto {
    public string CurrentSnuffId { get; set; }
    public string ImageUrl { get; set; }
    public string Brand { get; set; }
    public string Type { get; set; }
    public int AmountRemaing { get; set; }
    public DateTime PurchaseDate { get; set; }

    //ev lägga till pris
}