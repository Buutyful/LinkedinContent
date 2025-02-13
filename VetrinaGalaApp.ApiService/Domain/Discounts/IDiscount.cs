using VetrinaGalaApp.ApiService.Domain.UserDomain;

namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public interface IDiscount
{
    IEnumerable<DiscountApplication> GetAppliedDiscounts(Price appliedTo);
}
public record DiscountApplication(Price DiscountedAmount, Price AppliedTo, decimal DiscountPercentage);