using Sweet_Silvia_Application.Olva;
using Sweet_Silvia_Domain.Courier;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using Sweet_Silvia_Domain.ShippingRate;

namespace Sweet_Silvia_Tests;

public sealed class LocalDeliveryShippingTests
{
    [Fact]
    public void ExcludeDistrictsWithOwnDelivery_MatchesAccentsAndCercadoAliases()
    {
        var result = OlvaLocationNames.ExcludeDistrictsWithOwnDelivery(
            ["Jesus Maria", "Cercado de Lima", "San Miguel"],
            ["Jesús María", "Lima"]);

        Assert.Equal(["San Miguel"], result);
    }

    [Fact]
    public void CreateDistrictSpecificOwnDeliveryRate_AllowsProvinceDepartment()
    {
        var rate = ShippingRate.Create(
            Guid.NewGuid(),
            CourierType.OwnDelivery,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ShippingDestinationType.Province,
            ShippingServiceType.HomeDelivery,
            Money.Create(15m),
            null,
            "1 a 2 días");

        Assert.Equal(CourierType.OwnDelivery, rate.CourierType);
        Assert.Equal(ShippingDestinationType.Province, rate.DestinationType);
    }

    [Fact]
    public void CreateProvinceWideOwnDeliveryRate_RemainsInvalid()
    {
        Assert.Throws<ArgumentException>(() => ShippingRate.Create(
            Guid.NewGuid(),
            CourierType.OwnDelivery,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            ShippingDestinationType.Province,
            ShippingServiceType.HomeDelivery,
            Money.Create(15m),
            null,
            "1 a 2 días"));
    }
}
