using VetrinaGalaApp.ApiService.Domain.Common;
using VetrinaGalaApp.ApiService.Infrastructure.Models;
namespace VetrinaGalaApp.ApiService.Domain.UserDomain;

public enum Currency
{
    Euro,
    Dollars
}

public class DomainItem : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImgUrl { get; private set; }
    public Money Price { get; private set; }
    public RatingMetrics RatingMetrics { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid CatalogId { get; private set; }

    private DomainItem(
        Guid id,
        string name,
        string description,
        string imgUrl,
        Money price,
        RatingMetrics ratingMetrics,
        Guid storeId,
        Guid catalogId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Item name cannot be null or empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Item description cannot be null or empty");

        if (string.IsNullOrWhiteSpace(imgUrl))
            throw new InvalidOperationException("Item image URL cannot be null or empty");

        Name = name;
        Description = description;
        ImgUrl = imgUrl;
        Price = price;
        RatingMetrics = ratingMetrics;
        StoreId = storeId;
        CatalogId = catalogId;
    }

    public static DomainItem CreateNew(
        string name,
        string description,
        string imgUrl,
        Money price,
        Guid storeId,
        Guid catalogId) =>
        new(
            Guid.NewGuid(),
            name,
            description,
            imgUrl,
            price,
            new RatingMetrics(),
            storeId,
            catalogId);


    public static DomainItem FromItem(Item item) =>
        new(
            item.Id,
            item.Name,
            item.Description,
            item.ImgUrl,
            new Money(item.Price, item.Currency),
            new RatingMetrics(item.LikeCount, item.DislikeCount, item.MMR),
            item.StoreId,
            item.CatalogId);

}



public record ItemLiked(Guid ItemId, Guid StoreId) : IDomianEvent;