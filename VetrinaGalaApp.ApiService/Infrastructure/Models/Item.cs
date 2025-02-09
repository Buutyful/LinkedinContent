namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public Guid StoreId { get; set; }
    public Guid CatalogId { get; set; }
    public Catalog Catalog { get; set; }
    public Store Store { get; set; }
    public List<Discount> Discounts { get; set; } = new();
}
