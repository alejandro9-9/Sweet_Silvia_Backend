namespace Sweet_Silvia_Domain.Shared.ValueObjects;

public sealed record AddressLine
{
    private AddressLine(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AddressLine Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Address is required.", nameof(value));
        }

        value = value.Trim();

        if (value.Length > 250)
        {
            throw new ArgumentException("Address cannot exceed 250 characters.", nameof(value));
        }

        return new AddressLine(value);
    }
}
