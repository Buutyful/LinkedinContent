using VetrinaGalaApp.ApiService.Domain.UserDomain;

namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public class NoDiscount : IDiscount
{
    public IEnumerable<DiscountApplication> GetAppliedDiscounts(Money appliedTo) => [];
}
