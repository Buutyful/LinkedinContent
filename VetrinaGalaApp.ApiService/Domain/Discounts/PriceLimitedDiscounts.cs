using VetrinaGalaApp.ApiService.Domain.UserDomain;
namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public class PriceLimitedDiscounts : IDiscount
{
    public decimal MaxDiscountAmount { get; }
    public IDiscount Other { get; }
    
    public PriceLimitedDiscounts(IDiscount other, decimal cap)
    {
        if (cap <= 0 || cap >= 1)
            throw new InvalidOperationException("Invalid cap range");
        MaxDiscountAmount = cap;
        Other = other;        
    }
    public IEnumerable<DiscountApplication> GetAppliedDiscounts(Money applayedTo)
    {

        var minimumPrice = applayedTo -  (applayedTo * MaxDiscountAmount);
        var currentPrice = applayedTo;

        using var discountEnumerator = Other.GetAppliedDiscounts(currentPrice).GetEnumerator();

        while (discountEnumerator.MoveNext())
        {
            var discount = discountEnumerator.Current;

            if (currentPrice - discount.DiscountedAmount <= minimumPrice)
            {
                yield return discount with { DiscountedAmount = currentPrice - minimumPrice };
                yield break;               
            }

            yield return discount;
            currentPrice -= discount.DiscountedAmount;
        }
    }
}
