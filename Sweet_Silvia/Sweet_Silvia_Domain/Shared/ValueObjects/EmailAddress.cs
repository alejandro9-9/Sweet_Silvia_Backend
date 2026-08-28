namespace Sweet_Silvia_Domain.Shared.ValueObjects;

public sealed record EmailAddress
{
    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        value = value.Trim().ToLowerInvariant();

        if (!value.Contains('@') || !value.Contains('.'))
        {
            throw new ArgumentException("Email is invalid.", nameof(value));
        }

        return new EmailAddress(value);
    }
}
