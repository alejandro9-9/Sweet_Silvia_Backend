using System.Text.Json;
using Sweet_Silvia_Application.Olva;
using Sweet_Silvia_Application.ShippingRate;

namespace Sweet_Silvia_Tests;

public sealed class OlvaQuotePricingTests
{
    [Fact]
    public void CalculatePackage_GrowsStackHeightAndPreservesUnitVolume()
    {
        var package = OlvaQuotePricing.CalculatePackage([
            new ShippingPackageItem(3, 0.5m, 32m, 24m, 3m)
        ]);

        Assert.Equal(1.5m, package.ActualWeightKg);
        Assert.Equal(1.15m, package.VolumetricWeightKg);
        Assert.Equal(32m, package.LengthCm);
        Assert.Equal(24m, package.WidthCm);
        Assert.Equal(9m, package.HeightCm);
    }

    [Fact]
    public void CalculatePackage_CombinesDifferentProductsIntoOneBox()
    {
        var package = OlvaQuotePricing.CalculatePackage([
            new ShippingPackageItem(1, 0.5m, 32m, 24m, 3m),
            new ShippingPackageItem(2, 0.25m, 24m, 18m, 4m)
        ]);

        Assert.Equal(1m, package.ActualWeightKg);
        Assert.Equal(32m, package.LengthCm);
        Assert.Equal(24m, package.WidthCm);
        Assert.Equal(7.5m, package.HeightCm);
    }

    [Fact]
    public void ToPackageItem_UsesProgressiveFallbackWhenVariantMeasurementsAreMissing()
    {
        var item = OlvaQuotePricing.ToPackageItem(3, "Vestido Floral Rosa", 0m, 0m, 0m, 0m);
        var package = OlvaQuotePricing.CalculatePackage([item]);

        Assert.Equal(1.05m, package.ActualWeightKg);
        Assert.Equal(1.32m, package.VolumetricWeightKg);
        Assert.Equal(30m, package.LengthCm);
        Assert.Equal(22m, package.WidthCm);
        Assert.Equal(12m, package.HeightCm);
    }

    [Fact]
    public void TryReadCost_ReadsNestedNumericOrStringCost()
    {
        using var document = JsonDocument.Parse("""{"data":{"quote":{"total":"24.50"}}}""");

        Assert.True(OlvaQuotePricing.TryReadCost(document.RootElement, out var cost));
        Assert.Equal(24.50m, cost);
    }

    [Fact]
    public void TryReadCost_PrefersTotalOverRateRegardlessOfJsonPropertyOrder()
    {
        using var document = JsonDocument.Parse("""{"data":{"rate":0.18,"total":24.50}}""");

        Assert.True(OlvaQuotePricing.TryReadCost(document.RootElement, out var cost));
        Assert.Equal(24.50m, cost);
    }

    [Fact]
    public void TryReadCost_RejectsPayloadWithoutAUsablePrice()
    {
        using var document = JsonDocument.Parse("""{"data":{"status":"quoted","weight":1.2}}""");

        Assert.False(OlvaQuotePricing.TryReadCost(document.RootElement, out _));
    }
}
