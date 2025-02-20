namespace VetrinaGalaApp.ApiService.Domain.UserDomain;


public readonly record struct Money : IComparable<Money>
{
    public static Money Zero(Currency currency) => new(0, currency);
    public decimal Amount { get; init; }
    public Currency Currency { get; init; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        if (!Enum.IsDefined(currency))
            throw new ArgumentException($"Invalid currency: {currency}.", nameof(currency));
        Amount = amount;
        Currency = currency;
    }

    public int CompareTo(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare different currencies.");
        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add different currencies.");
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies.");

        decimal result = left.Amount - right.Amount;
        return result < 0 ? Zero(left.Currency) : new Money(result, left.Currency);
    }

    public static Money operator *(Money price, decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be non-negative.");
        return new Money(price.Amount * multiplier, price.Currency);
    }

    public static Money operator /(Money price, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Divisor cannot be zero.");
        return new Money(price.Amount / divisor, price.Currency);
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount <= right.Amount;
    }

    public override string ToString() => $"{Amount} {Currency}";

    
}
