namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImgUrl { get; set; } = null!;
    public decimal Price { get; set; }

    //mmr currently could be as simple as 1 like + 10, 1 dislike -1, and could be the key for sorting priority
    public int MMR { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    

    // Relationships
    public Guid StoreId { get; set; }
    public Guid CatalogId { get; set; }
    public Catalog? Catalog { get; set; }
    public Store? Store { get; set; }
    public List<Discount> Discounts { get; set; } = new();
}
