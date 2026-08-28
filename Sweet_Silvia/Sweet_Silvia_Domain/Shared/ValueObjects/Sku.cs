namespace Sweet_Silvia_Domain.Shared.ValueObjects;

public sealed record Sku
{
    private Sku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("SKU is required.", nameof(value));
        }

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException("SKU cannot exceed 50 characters.", nameof(value));
        }

        if (!value.All(character => char.IsLetterOrDigit(character) || character == '-'))
        {
            throw new ArgumentException("SKU should contain only letters, numbers or hyphens.", nameof(value));
        }

        return new Sku(value);
    }
}
