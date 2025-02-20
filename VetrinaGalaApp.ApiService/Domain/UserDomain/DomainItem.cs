using VetrinaGalaApp.ApiService.Domain.Common;
using VetrinaGalaApp.ApiService.Infrastructure.Models;
namespace VetrinaGalaApp.ApiService.Domain.UserDomain;

public enum Currency
{
    Euro,
    Dollars
}
public class RatingMetrics
{
    public const int DefaultInitialMmr = 1500;
    public const int DefaultKFactor = 32;

    public int MMR { get; private set; }
    public int LikeCount { get; private set; }
    public int DislikeCount { get; private set; }
    public double PositiveRatio =>
        LikeCount + DislikeCount == 0 ?
        0.5 : (double)LikeCount / (LikeCount + DislikeCount);

    public RatingMetrics(int likeCount = 0, int dislikeCount = 0, int initialMMR = DefaultInitialMmr)
    {
        MMR = initialMMR;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
    }

    public void AddLike()
    {
        LikeCount++;
        MMR = CalculateNewMmr();
    }

    public void AddDislike()
    {
        DislikeCount++;
        MMR = CalculateNewMmr();
    }  
    private int CalculateNewMmr()
    {
        double expectedScore = 1.0 / (1.0 + Math.Pow(10, (DefaultInitialMmr - MMR) / 400.0));       

        var newMmr = MMR + (int)(DefaultKFactor * (PositiveRatio - expectedScore));
        return Math.Max(0, newMmr);
    }
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