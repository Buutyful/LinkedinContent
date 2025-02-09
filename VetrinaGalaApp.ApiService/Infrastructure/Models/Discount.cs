namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Discount
{
    public Guid Id { get; set; }
    public decimal Percentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public Guid StoreId { get; set; }
    public Store Store { get; set; }

    public Guid ItemId { get; set; }
    public Item Item { get; set; }
}