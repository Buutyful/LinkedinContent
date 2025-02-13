namespace VetrinaGalaApp.ApiService.Domain.UserDomain;

public readonly record struct Price : IComparable<Price>
{
    public static Price Zero(Currency currency) => new(0, currency);
    public decimal Amount { get; init; }
    public Currency Currency { get; init; }

    public Price(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new InvalidOperationException("Item price must be greater than zero");
        this.Amount = amount;
        this.Currency = currency;
    }

    public int CompareTo(Price other)
    {
        if (other.Currency != this.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return this.Amount.CompareTo(other.Amount);
    }

    public static Price operator +(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant add different currencies");
        return new Price(left.Amount + right.Amount, left.Currency);
    }   
    public static Price operator -(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant subtract different currencies");
        var result = left.Amount - right.Amount;
        if (result <= 0)
            return Zero(left.Currency);
        return new Price(result, left.Currency);
    }

    public static Price operator *(Price price, decimal multiplier)
    {
        if (multiplier <= 0)
            throw new InvalidOperationException("Multiplier must be greater than zero");
        return new Price(price.Amount * multiplier, price.Currency);
    }

    public static Price operator /(Price price, decimal divisor)
    {
        if (divisor <= 0)
            throw new InvalidOperationException("Divisor must be greater than zero");
        return new Price(price.Amount / divisor, price.Currency);
    }
    
    public static bool operator >(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("cant compare different currencies");
        return left.Amount <= right.Amount;
    }

    public override string ToString() => $"{Amount} {Currency}";

    
}
