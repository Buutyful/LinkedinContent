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
    public decimal Price { get; private set; }
    public int MMR { get; private set; }
    public int LikeCount { get; private set; }
    public int DislikeCount { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid CatalogId { get; private set; }

    private DomainItem(
        Guid id,
        string name,
        string description,
        string imgUrl,
        decimal price,
        int mmr,
        int likeCount,
        int dislikeCount,
        Guid storeId,
        Guid catalogId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Item name cannot be null or empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Item description cannot be null or empty");

        if (string.IsNullOrWhiteSpace(imgUrl))
            throw new InvalidOperationException("Item image URL cannot be null or empty");

        if (price <= 0)
            throw new InvalidOperationException("Item price must be greater than zero");

        Name = name;
        Description = description;
        ImgUrl = imgUrl;
        Price = price;
        MMR = mmr;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
        StoreId = storeId;
        CatalogId = catalogId;
    }

    public static DomainItem CreateNew(
        string name,
        string description,
        string imgUrl,
        decimal price,
        Guid storeId,
        Guid catalogId)
    {
        return new DomainItem(
            Guid.NewGuid(),
            name,
            description,
            imgUrl,
            price,
            0, // Initial MMR
            0, // Initial like count
            0, // Initial dislike count
            storeId,
            catalogId);
    }

    public static DomainItem FromItem(Item item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        return new DomainItem(
            item.Id,
            item.Name,
            item.Description,
            item.ImgUrl,
            item.Price,
            item.MMR,
            item.LikeCount,
            item.DislikeCount,
            item.StoreId,
            item.CatalogId);
    }

    public void UpdateMMR(bool isLike)
    {
        if (isLike)
        {
            MMR += 10;
            LikeCount++;
            AddEvent(new ItemLiked(Id, StoreId));
        }
        else
        {
            MMR -= 1;
            DislikeCount++;
        }
    }
}


public record ItemLiked(Guid ItemId, Guid StoreId) : IDomianEvent;