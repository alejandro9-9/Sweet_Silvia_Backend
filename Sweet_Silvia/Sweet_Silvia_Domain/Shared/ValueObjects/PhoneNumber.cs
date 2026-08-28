namespace Sweet_Silvia_Domain.Shared.ValueObjects;

public sealed record PhoneNumber
{
    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone number is required.", nameof(value));
        }

        value = value.Trim().Replace(" ", string.Empty);

        if (value.StartsWith("+51", StringComparison.Ordinal))
        {
            value = value[3..];
        }

        if (value.Length != 9)
        {
            throw new ArgumentException("Phone number should have 9 digits.", nameof(value));
        }

        if (!value.All(char.IsDigit))
        {
            throw new ArgumentException("Phone number should contain only digits.", nameof(value));
        }

        return new PhoneNumber($"+51{value}");
    }
}
