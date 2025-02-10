namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsVerified { get; set; }
   

    // Relationships
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public List<Item> Items { get; set; } = new();
    public List<Discount> Discounts { get; set; } = new();
   
}

