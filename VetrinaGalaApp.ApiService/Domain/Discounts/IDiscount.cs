using System;
using VetrinaGalaApp.ApiService.Application.StoreUseCases.Services;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public interface IDiscount
{
    IEnumerable<DiscountApplication> GetAppliedDiscounts(Money appliedTo);
}
public record DiscountApplication(Money DiscountedAmount, Money AppliedTo, decimal DiscountPercentage);

public static class DiscountExtentions
{
    public static IDiscount CreateDiscountStrategy(
        this IList<Discount> discounts,
        decimal discountCap = 0.40m) =>
        discounts switch
        {
            [] => new NoDiscount(),
            [var single] => new SingleDiscount(single.Percentage),
            var many => new PriceLimitedDiscounts
                        (
                            new ChainedDiscounts(
                                discounts
                                .Select(d => new SingleDiscount(d.Percentage))),
                            discountCap
                        )
        };
    public static Money FinalDiscountPrice(
            this IEnumerable<DiscountApplication> discountApplications,
            Currency currency) =>
            discountApplications
            .Aggregate(Money.Zero(currency),
                (cur, next) => cur + next.DiscountedAmount);
}