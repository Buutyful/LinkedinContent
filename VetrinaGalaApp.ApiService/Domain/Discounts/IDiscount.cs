using VetrinaGalaApp.ApiService.Domain.UserDomain;

namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public interface IDiscount
{
    IEnumerable<DiscountApplication> GetAppliedDiscounts(Money appliedTo);
}
public record DiscountApplication(Money DiscountedAmount, Money AppliedTo, decimal DiscountPercentage);

public static class DiscountExtentions
{   
    public static IDiscount CreateDiscountStrategy(
        this IList<DiscountDto> discounts,
        decimal discountCap = 0.40m) =>
        discounts switch
        {
            [] => new NoDiscount(),
            [var single] => new SingleDiscount(single.Percentage),
            var many => new PriceLimitedDiscounts
                        (
                            new ChainedDiscounts(
                                many.Select(d => new SingleDiscount(d.Percentage))),
                                discountCap
                        )
        };

    //Returns the final price after all discounts are applied
    public static Money FinalDiscountedAmount(
            this IEnumerable<DiscountApplication> discountApplications,
            Currency currency) =>
            discountApplications
            .Aggregate(Money.Zero(currency),
                (cur, next) => cur + next.DiscountedAmount);
}

public record DiscountDto(decimal Percentage);