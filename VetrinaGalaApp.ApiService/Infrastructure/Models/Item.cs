using VetrinaGalaApp.ApiService.Domain.UserDomain;

namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class Item
{
    public Guid Id { get; set; }

    //metadata
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImgUrl { get; set; } = null!;

    //money
    public decimal Price { get; set; }
    public Currency Currency { get; set; }

    //rating metrics
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
public static class ItemMappings
{
    public static Item FromDomainItem(this DomainItem domainItem) =>
         new()
         {
             Id = domainItem.Id,
             Name = domainItem.Name,
             Description = domainItem.Description,
             ImgUrl = domainItem.ImgUrl,
             Price = domainItem.Price.Amount,
             Currency = domainItem.Price.Currency,
             MMR = domainItem.RatingMetrics.MMR,
             LikeCount = domainItem.RatingMetrics.LikeCount,
             DislikeCount = domainItem.RatingMetrics.DislikeCount,
             StoreId = domainItem.StoreId,
             CatalogId = domainItem.CatalogId
         };


}