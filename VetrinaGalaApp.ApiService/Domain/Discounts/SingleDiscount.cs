﻿using VetrinaGalaApp.ApiService.Domain.UserDomain;
namespace VetrinaGalaApp.ApiService.Domain.Discounts;

public class SingleDiscount : IDiscount
{
    public decimal AmountPercentage { get; }
    public SingleDiscount(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Discount must be greater than zero");
        if (amount > 1)
            throw new InvalidOperationException("Discount cant surpass the toal price");
        AmountPercentage = amount;
    }

    public IEnumerable<DiscountApplication> GetAppliedDiscounts(Money applayedTo)
    {
        var discountAmount = applayedTo * AmountPercentage;
        yield return new DiscountApplication(discountAmount, applayedTo, AmountPercentage);
    }
}
