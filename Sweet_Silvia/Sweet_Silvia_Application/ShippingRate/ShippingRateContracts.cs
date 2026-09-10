using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Courier;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.ShippingRate;

namespace Sweet_Silvia_Application.ShippingRate;

public sealed record ShippingRateResponse(Guid Id, Guid CourierId, CourierType CourierType, Guid DepartmentId, Guid? ProvinceId, Guid? DistrictId, ShippingDestinationType DestinationType, ShippingServiceType ServiceType, decimal Cost, string Currency, decimal? FreeShippingMinimumAmount, string? EstimatedTime, bool IsActive);

public sealed record CreateShippingRateCommand(Guid CourierId, CourierType CourierType, Guid DepartmentId, Guid? ProvinceId, Guid? DistrictId, ShippingDestinationType DestinationType, ShippingServiceType ServiceType, decimal Cost, string Currency = "PEN", decimal? FreeShippingMinimumAmount = null, string? EstimatedTime = null) : ICommand<Guid>;

public sealed record UpdateShippingRateCommand(Guid Id, Guid CourierId, CourierType CourierType, Guid DepartmentId, Guid? ProvinceId, Guid? DistrictId, ShippingDestinationType DestinationType, ShippingServiceType ServiceType, decimal Cost, string Currency = "PEN", decimal? FreeShippingMinimumAmount = null, string? EstimatedTime = null) : ICommand;

public sealed record DeleteShippingRateCommand(Guid Id) : ICommand;

public sealed record ShippingPackageItem(int Quantity, decimal? WeightKg, decimal? LengthCm, decimal? WidthCm, decimal? HeightCm);

public sealed record CalculateShippingCostQuery(
    Guid DepartmentId,
    Guid? ProvinceId,
    Guid? DistrictId,
    ShippingDestinationType DestinationType,
    decimal OrderSubtotal,
    string Currency = "PEN",
    IReadOnlyList<ShippingPackageItem>? PackageItems = null,
    Guid? CartUserId = null) : IQuery<ShippingCostResponse>;

public sealed record ShippingCostResponse(
    Guid ShippingRateId,
    Guid CourierId,
    decimal Cost,
    string Currency,
    string? EstimatedTime,
    decimal ChargeableWeightKg,
    decimal VolumetricWeightKg,
    decimal ActualWeightKg,
    decimal VolumeSurcharge);

public sealed record GetShippingRatesQuery(Guid? DepartmentId = null, Guid? ProvinceId = null, Guid? DistrictId = null, ShippingDestinationType? DestinationType = null, bool OnlyActive = true) : IQuery<List<ShippingRateResponse>>;
