using System.Globalization;
using System.Text.Json;
using Sweet_Silvia_Application.ShippingRate;

namespace Sweet_Silvia_Application.Olva;

public sealed record OlvaPackageDimensions(
    decimal ActualWeightKg,
    decimal VolumetricWeightKg,
    decimal ChargeableWeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm);

public static class OlvaQuotePricing
{
    public const string CheckoutOriginUbigeo = "150136";
    public const bool CheckoutPartnerRate = false;

    private static readonly string[] PricePropertyPriority =
    {
        "total", "totalamount", "amounttotal", "amount", "price", "cost", "tariff", "monto", "importe", "rate"
    };

    public static OlvaPackageDimensions CalculatePackage(IReadOnlyCollection<ShippingPackageItem> items)
    {
        var weight = items.Sum(item => (item.WeightKg ?? 0m) * item.Quantity);
        var volume = items.Sum(item => (item.LengthCm ?? 0m) * (item.WidthCm ?? 0m) * (item.HeightCm ?? 0m) * item.Quantity);
        var length = items.Count == 0 ? 0 : items.Max(item => item.LengthCm ?? 0m);
        var width = items.Count == 0 ? 0 : items.Max(item => item.WidthCm ?? 0m);
        var height = length > 0 && width > 0 ? volume / (length * width) : 0;
        var volumetricWeight = volume / 6000m;

        return new OlvaPackageDimensions(
            Round(weight),
            Round(volumetricWeight),
            Round(Math.Max(weight, volumetricWeight)),
            Round(length),
            Round(width),
            Round(height));
    }

    public static ShippingPackageItem ToPackageItem(
        int quantity,
        string productName,
        decimal weightKg,
        decimal lengthCm,
        decimal widthCm,
        decimal heightCm)
    {
        var profile = GetFallbackProfile(productName);
        return new ShippingPackageItem(
            quantity,
            PositiveOrFallback(weightKg, profile.WeightKg),
            PositiveOrFallback(lengthCm, profile.LengthCm),
            PositiveOrFallback(widthCm, profile.WidthCm),
            PositiveOrFallback(heightCm, profile.HeightCm));
    }

    public static bool TryReadCost(JsonElement data, out decimal cost)
    {
        cost = 0;
        return FindCost(data, out cost) && cost > 0;
    }

    private static bool FindCost(JsonElement element, out decimal cost)
    {
        cost = 0;
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var candidate in PricePropertyPriority)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (NormalizePropertyName(property.Name) == candidate && TryReadDecimal(property.Value, out cost) && cost > 0)
                    {
                        return true;
                    }
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                if (FindCost(property.Value, out cost))
                {
                    return true;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (FindCost(item, out cost))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryReadDecimal(JsonElement value, out decimal number)
    {
        number = 0;
        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.TryGetDecimal(out number);
        }

        return value.ValueKind == JsonValueKind.String &&
               decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number);
    }

    private static string NormalizePropertyName(string value)
        => string.Concat(value.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal PositiveOrFallback(decimal value, decimal fallback) => value > 0 ? value : fallback;

    private static (decimal WeightKg, decimal LengthCm, decimal WidthCm, decimal HeightCm) GetFallbackProfile(string productName)
    {
        var name = productName.ToLowerInvariant();
        if (name.Contains("sandalia") || name.Contains("zapato")) return (0.75m, 34m, 24m, 12m);
        if (name.Contains("bolso") || name.Contains("cartera")) return (0.65m, 32m, 24m, 12m);
        if (name.Contains("blazer") || name.Contains("chaqueta") || name.Contains("cardigan")) return (0.8m, 38m, 30m, 8m);
        if (name.Contains("jean") || name.Contains("pantalon")) return (0.65m, 34m, 26m, 7m);
        if (name.Contains("gorra") || name.Contains("lente")) return (0.25m, 24m, 18m, 12m);
        return (0.35m, 30m, 22m, 4m);
    }
}
