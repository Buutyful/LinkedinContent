namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Swipe
{
    public Guid Id { get; set; }
    public bool IsLike { get; set; }
    public DateTime SwipedAt { get; set; }

    // Relationships
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid ItemId { get; set; }
    public Item? Item { get; set; }
}
