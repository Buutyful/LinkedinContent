namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public Guid UserId { get; set; }
    public User User { get; set; }
    public List<Item> Items { get; set; } = new();
    public List<Discount> Discounts { get; set; } = new();
    public List<Catalog> Catalogs { get; set; } = new();
}
