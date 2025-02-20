using VetrinaGalaApp.ApiService.Domain.UserDomain;
namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public class ChainedDiscounts(params IEnumerable<IDiscount> discounts) : IDiscount
{
    private readonly IEnumerable<IDiscount> _discounts = discounts;
    public IEnumerable<DiscountApplication> GetAppliedDiscounts(Money applayedTo)
    {

        var resultingPirce = applayedTo;

        foreach (var discount in _discounts)
        {
            foreach (var applayedDiscount in discount.GetAppliedDiscounts(resultingPirce))
            {
                yield return applayedDiscount;

                resultingPirce -= applayedDiscount.DiscountedAmount;

                if (resultingPirce == Money.Zero(resultingPirce.Currency))
                    yield break;
            }
        }
    }
}
