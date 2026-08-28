namespace Sweet_Silvia_Domain.Shared.ValueObjects;

public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Zero(string currency = "PEN") => Create(0m, currency);

    public static Money Create(decimal amount, string currency = "PEN")
    {
        if (amount < 0)
        {
            throw new ArgumentException("Money amount cannot be negative.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        return new Money(decimal.Round(amount, 2), currency.Trim().ToUpperInvariant());
    }
}
