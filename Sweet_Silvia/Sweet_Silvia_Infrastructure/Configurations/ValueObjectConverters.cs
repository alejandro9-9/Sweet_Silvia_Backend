using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class AddressLineConverter()
    : ValueConverter<AddressLine, string>(address => address.Value, value => AddressLine.Create(value));

internal sealed class CouponCodeConverter()
    : ValueConverter<CouponCode, string>(code => code.Value, value => CouponCode.Create(value));

internal sealed class EmailAddressConverter()
    : ValueConverter<EmailAddress, string>(email => email.Value, value => EmailAddress.Create(value));

internal sealed class MoneyConverter()
    : ValueConverter<Money, string>(money => $"{money.Amount}|{money.Currency}", value => Parse(value))
{
    internal static Money Parse(string value)
    {
        var parts = value.Split('|', 2);
        return Money.Create(decimal.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture), parts[1]);
    }
}

internal sealed class NullableMoneyConverter()
    : ValueConverter<Money?, string?>(
        money => money == null ? null : $"{money.Amount}|{money.Currency}",
        value => string.IsNullOrWhiteSpace(value) ? null : MoneyConverter.Parse(value));

internal sealed class PhoneNumberConverter()
    : ValueConverter<PhoneNumber, string>(phone => phone.Value, value => PhoneNumber.Create(value));

internal sealed class NullablePhoneNumberConverter()
    : ValueConverter<PhoneNumber?, string?>(
        phone => phone == null ? null : phone.Value,
        value => string.IsNullOrWhiteSpace(value) ? null : PhoneNumber.Create(value));

internal sealed class SkuConverter()
    : ValueConverter<Sku, string>(sku => sku.Value, value => Sku.Create(value));
