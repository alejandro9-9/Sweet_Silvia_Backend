namespace Sweet_Silvia_Domain.Shared.ValueObjects;

public sealed record CouponCode
{
    private CouponCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CouponCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The coupon code is required.", nameof(value));
        }

        value = value.Trim().ToUpperInvariant();

        if (value.Length != 6)
        {
            throw new ArgumentException("The coupon code should have 6 characters.", nameof(value));
        }

        if (!value.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException("The coupon code should contain only letters and numbers.", nameof(value));
        }

        return new CouponCode(value);
    }
}
