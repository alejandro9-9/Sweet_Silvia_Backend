using Sweet_Silvia_Domain.ProductVariant;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Tests;

public sealed class ProductVariantGarmentMeasurementTests
{
    [Fact]
    public void Create_StoresOptionalGarmentMeasurementsSeparatelyFromShippingDimensions()
    {
        var variant = ProductVariant.Create(
            Guid.NewGuid(),
            "M",
            "Rosa",
            Sku.Create("VEST-ROS-M"),
            10,
            chestCircumferenceCm: 76m,
            waistCircumferenceCm: 74m,
            hipCircumferenceCm: 92m,
            sleeveLengthCm: 63m,
            frontLengthCm: 116m,
            backLengthCm: 116m);

        Assert.Equal(76m, variant.ChestCircumferenceCm);
        Assert.Equal(74m, variant.WaistCircumferenceCm);
        Assert.Equal(92m, variant.HipCircumferenceCm);
        Assert.Equal(63m, variant.SleeveLengthCm);
        Assert.Equal(116m, variant.FrontLengthCm);
        Assert.Equal(116m, variant.BackLengthCm);
        Assert.Equal(0.3m, variant.ShippingWeightKg);
        Assert.Equal(30m, variant.ShippingLengthCm);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(501)]
    public void Create_RejectsGarmentMeasurementsOutsideTheAllowedRange(int measurement)
    {
        Assert.Throws<ArgumentException>(() => ProductVariant.Create(
            Guid.NewGuid(),
            "M",
            "Rosa",
            Sku.Create("VEST-ROS-M"),
            10,
            chestCircumferenceCm: measurement));
    }
}
