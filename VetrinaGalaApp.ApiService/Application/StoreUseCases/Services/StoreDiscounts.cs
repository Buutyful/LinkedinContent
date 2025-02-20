using Microsoft.EntityFrameworkCore;
using VetrinaGalaApp.ApiService.Domain.Discounts;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.ApiService.Application.StoreUseCases.Services;

public record ItemPriceLable(
    Money OriginalPrice,
    Money FinalPrice,
    params IEnumerable<DiscountApplication> DiscountApplications);
public interface IStoreDiscounts
{
    ItemPriceLable GetDiscountsResult(DomainItem item);
}

public class StoreDiscounts
    (AppDbContext context) : IStoreDiscounts
{
    private readonly AppDbContext _context = context;
    public ItemPriceLable GetDiscountsResult(DomainItem item)
    {
        var storeDiscounts =
             _context.Discounts
             .AsNoTracking()
             .Where(x => x.StoreId == item.StoreId)
             .ToList();

        //TODO: add a store field for the discount cap allowed on store items

        IDiscount discount = storeDiscounts.CreateDiscountStrategy();

        var discApllications = discount.GetAppliedDiscounts(item.Price);

        var finalPrice = discApllications.FinalDiscountPrice(item.Price.Currency);

        return new ItemPriceLable(item.Price, finalPrice, discApllications);
    }
}
