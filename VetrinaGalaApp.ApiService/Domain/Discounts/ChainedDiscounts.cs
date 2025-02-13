using VetrinaGalaApp.ApiService.Domain.UserDomain;
namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public class ChainedDiscounts(params IEnumerable<IDiscount> discounts) : IDiscount
{
    private readonly IEnumerable<IDiscount> _discounts = discounts;
    public IEnumerable<DiscountApplication> GetAppliedDiscounts(Price applayedTo)
    {

        var resultingPirce = applayedTo;

        foreach (var discount in _discounts)
        {
            foreach (var applayedDiscount in discount.GetAppliedDiscounts(resultingPirce))
            {
                yield return applayedDiscount;

                resultingPirce -= applayedDiscount.DiscountedAmount;

                if (resultingPirce == Price.Zero(resultingPirce.Currency))
                    yield break;
            }
        }
    }
}
