namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Discount
{
    public Guid Id { get; set; }
    public decimal Percentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }


    // Relationships
    public Guid StoreId { get; set; }
    public Store? Store { get; set; }

    public Guid ItemId { get; set; }
    public Item? Item { get; set; }
}


