using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.Shipment;
using Sweet_Silvia_Application.ShipmentEvent;
using Sweet_Silvia_Application.ShippingRate;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Cart;
using Sweet_Silvia_Domain.CartItem;
using Sweet_Silvia_Domain.Courier;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using Sweet_Silvia_Domain.Shipment;
using CourierEntity = Sweet_Silvia_Domain.Courier.Courier;
using DepartmentEntity = Sweet_Silvia_Domain.Department.Department;
using DistrictEntity = Sweet_Silvia_Domain.District.District;
using ProvinceEntity = Sweet_Silvia_Domain.Province.Province;
using ShipmentEntity = Sweet_Silvia_Domain.Shipment.Shipment;
using ShipmentEventEntity = Sweet_Silvia_Domain.ShipmentEvent.ShipmentEvent;
using ShippingRateEntity = Sweet_Silvia_Domain.ShippingRate.ShippingRate;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using OrderStatusHistoryEntity = Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using CartEntity = Sweet_Silvia_Domain.Cart.Cart;
using CartItemEntity = Sweet_Silvia_Domain.CartItem.CartItem;
using OlvaAgencyEntity = Sweet_Silvia_Domain.OlvaAgency.OlvaAgency;

namespace Sweet_Silvia_Application.Courier;

internal sealed class CreateCourierCommandHandler(IRepository<CourierEntity> couriers, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCourierCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = CourierEntity.Create(request.Name, request.Type, request.RequiresExternalIntegration);
        couriers.Add(courier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return courier.Id;
    }
}

internal sealed class UpdateCourierCommandHandler(IRepository<CourierEntity> couriers, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCourierCommand>
{
    public async Task<Result> Handle(UpdateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await couriers.GetByIdAsync(request.Id, cancellationToken);
        if (courier is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Courier", request.Id));
        }

        courier.Update(request.Name, request.Type, request.RequiresExternalIntegration);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteCourierCommandHandler(IRepository<CourierEntity> couriers, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCourierCommand>
{
    public async Task<Result> Handle(DeleteCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await couriers.GetByIdAsync(request.Id, cancellationToken);
        if (courier is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Courier", request.Id));
        }

        courier.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetCourierByIdQueryHandler(IRepository<CourierEntity> couriers)
    : IQueryHandler<GetCourierByIdQuery, CourierResponse>
{
    public Task<Result<CourierResponse>> Handle(GetCourierByIdQuery request, CancellationToken cancellationToken)
    {
        var courier = couriers.Query().FirstOrDefault(courier => courier.Id == request.Id);
        return Task.FromResult(courier is null
            ? Result.Failure<CourierResponse>(ApplicationErrors.NotFound("Courier", request.Id))
            : Result.Success(ShippingMappings.ToCourierResponse(courier)));
    }
}

internal sealed class GetCouriersQueryHandler(IRepository<CourierEntity> couriers)
    : IQueryHandler<GetCouriersQuery, List<CourierResponse>>
{
    public Task<Result<List<CourierResponse>>> Handle(GetCouriersQuery request, CancellationToken cancellationToken)
    {
        var query = couriers.Query();
        if (request.OnlyActive)
        {
            query = query.Where(courier => courier.IsActive);
        }

        return Task.FromResult(Result.Success(query.Select(ShippingMappings.ToCourierResponse).ToList()));
    }
}

internal sealed class CreateShippingRateCommandHandler(
    IRepository<CourierEntity> couriers,
    IRepository<ShippingRateEntity> rates,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateShippingRateCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateShippingRateCommand request, CancellationToken cancellationToken)
    {
        var validation = ShippingMappings.ValidateShippingLocation(request.DepartmentId, request.ProvinceId, request.DistrictId, request.DestinationType, departments, provinces, districts);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Error);
        }

        var courier = await couriers.GetByIdAsync(request.CourierId, cancellationToken);
        if (courier is null || !courier.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Courier", request.CourierId));
        }

        if (courier.Type != request.CourierType)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipping.CourierTypeMismatch",
                "The selected courier type does not match the configured courier."));
        }

        var rate = ShippingRateEntity.Create(
            request.CourierId,
            request.CourierType,
            request.DepartmentId,
            request.ProvinceId,
            request.DistrictId,
            request.DestinationType,
            request.ServiceType,
            Money.Create(request.Cost, request.Currency),
            request.FreeShippingMinimumAmount.HasValue ? Money.Create(request.FreeShippingMinimumAmount.Value, request.Currency) : null,
            request.EstimatedTime);

        rates.Add(rate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return rate.Id;
    }
}

internal sealed class UpdateShippingRateCommandHandler(
    IRepository<CourierEntity> couriers,
    IRepository<ShippingRateEntity> rates,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateShippingRateCommand>
{
    public async Task<Result> Handle(UpdateShippingRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await rates.GetByIdAsync(request.Id, cancellationToken);
        if (rate is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ShippingRate", request.Id));
        }

        var validation = ShippingMappings.ValidateShippingLocation(request.DepartmentId, request.ProvinceId, request.DistrictId, request.DestinationType, departments, provinces, districts);
        if (validation.IsFailure)
        {
            return validation;
        }

        var courier = await couriers.GetByIdAsync(request.CourierId, cancellationToken);
        if (courier is null || !courier.IsActive)
        {
            return Result.Failure(ApplicationErrors.NotFound("Courier", request.CourierId));
        }

        if (courier.Type != request.CourierType)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Shipping.CourierTypeMismatch",
                "The selected courier type does not match the configured courier."));
        }

        rate.Update(
            request.CourierId,
            request.CourierType,
            request.DepartmentId,
            request.ProvinceId,
            request.DistrictId,
            request.DestinationType,
            request.ServiceType,
            Money.Create(request.Cost, request.Currency),
            request.FreeShippingMinimumAmount.HasValue ? Money.Create(request.FreeShippingMinimumAmount.Value, request.Currency) : null,
            request.EstimatedTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteShippingRateCommandHandler(IRepository<ShippingRateEntity> rates, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteShippingRateCommand>
{
    public async Task<Result> Handle(DeleteShippingRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await rates.GetByIdAsync(request.Id, cancellationToken);
        if (rate is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ShippingRate", request.Id));
        }

        rate.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class CalculateShippingCostQueryHandler(
    IRepository<ShippingRateEntity> rates,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts,
    IRepository<CourierEntity> couriers,
    IRepository<CartEntity> carts,
    IRepository<CartItemEntity> cartItems,
    IRepository<ProductVariantEntity> variants)
    : IQueryHandler<CalculateShippingCostQuery, ShippingCostResponse>
{
    public Task<Result<ShippingCostResponse>> Handle(CalculateShippingCostQuery request, CancellationToken cancellationToken)
    {
        var locationValidation = ShippingMappings.ValidateShippingLocation(
            request.DepartmentId,
            request.ProvinceId,
            request.DistrictId,
            request.DestinationType,
            departments,
            provinces,
            districts);
        if (locationValidation.IsFailure)
        {
            return Task.FromResult(Result.Failure<ShippingCostResponse>(locationValidation.Error));
        }

        var rate = rates.Query()
            .Where(rate => rate.IsActive && couriers.Query().Any(courier => courier.Id == rate.CourierId && courier.IsActive))
            .AsEnumerable()
            .Where(rate => rate.AppliesTo(request.DepartmentId, request.ProvinceId, request.DistrictId, request.DestinationType))
            .OrderByDescending(rate => rate.DistrictId.HasValue)
            .ThenByDescending(rate => rate.ProvinceId.HasValue)
            .FirstOrDefault();

        if (rate is null)
        {
            return Task.FromResult(Result.Failure<ShippingCostResponse>(ApplicationErrors.NotFound("ShippingRate")));
        }

        var baseCost = rate.CalculateCost(Money.Create(request.OrderSubtotal, request.Currency));
        // Package measurements must come from server-owned catalog data. Until
        // dimensions are stored per variant, use one deterministic quantity-based
        // package for both the quote and order creation paths.
        var packageItems = request.CartUserId.HasValue
            ? (from cart in carts.Query()
               join item in cartItems.Query() on cart.Id equals item.CartId
               join variant in variants.Query() on item.ProductVariantId equals variant.Id
               where cart.UserId == request.CartUserId.Value && cart.IsActive && variant.IsActive
               select new ShippingPackageItem(item.Quantity, variant.ShippingWeightKg, variant.ShippingLengthCm, variant.ShippingWidthCm, variant.ShippingHeightCm)).ToList()
            : null;
        var volumePricing = request.CartUserId.HasValue
            ? ShippingVolumeCalculator.Calculate(request.DestinationType, packageItems)
            : ShippingVolumeCalculator.CalculateForQuantity(request.DestinationType, request.PackageItems?.Sum(item => Math.Max(item.Quantity, 0)) ?? 0);
        var finalCost = Money.Create(baseCost.Amount + volumePricing.Surcharge, baseCost.Currency);

        return Task.FromResult(Result.Success(new ShippingCostResponse(
            rate.Id,
            rate.CourierId,
            finalCost.Amount,
            finalCost.Currency,
            rate.EstimatedTime,
            volumePricing.ChargeableWeightKg,
            volumePricing.VolumetricWeightKg,
            volumePricing.ActualWeightKg,
            volumePricing.Surcharge)));
    }
}

internal sealed class GetShippingRatesQueryHandler(
    IRepository<ShippingRateEntity> rates,
    IRepository<CourierEntity> couriers)
    : IQueryHandler<GetShippingRatesQuery, List<ShippingRateResponse>>
{
    public Task<Result<List<ShippingRateResponse>>> Handle(GetShippingRatesQuery request, CancellationToken cancellationToken)
    {
        var query = rates.Query();
        if (request.OnlyActive)
        {
            query = query.Where(rate => rate.IsActive &&
                couriers.Query().Any(courier => courier.Id == rate.CourierId && courier.IsActive));
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(rate => rate.DepartmentId == request.DepartmentId.Value);
        }

        if (request.ProvinceId.HasValue)
        {
            query = query.Where(rate => rate.ProvinceId == request.ProvinceId.Value);
        }

        if (request.DistrictId.HasValue)
        {
            query = query.Where(rate => rate.DistrictId == request.DistrictId.Value);
        }

        if (request.DestinationType.HasValue)
        {
            query = query.Where(rate => rate.DestinationType == request.DestinationType.Value);
        }

        return Task.FromResult(Result.Success(query.Select(ShippingMappings.ToRateResponse).ToList()));
    }
}

internal sealed record ShippingVolumePricing(decimal ActualWeightKg, decimal VolumetricWeightKg, decimal ChargeableWeightKg, decimal Surcharge);

internal static class ShippingVolumeCalculator
{
    private const decimal VolumetricFactorCm3 = 6000m;
    private const decimal DefaultItemWeightKg = 0.3m;
    private const decimal DefaultItemLengthCm = 30m;
    private const decimal DefaultItemWidthCm = 22m;
    private const decimal DefaultItemHeightCm = 4m;

    public static ShippingVolumePricing Calculate(ShippingDestinationType destinationType, IReadOnlyList<ShippingPackageItem>? items)
    {
        var packageItems = items is { Count: > 0 }
            ? items.Where(item => item.Quantity > 0).ToList()
            : [new ShippingPackageItem(1, DefaultItemWeightKg, DefaultItemLengthCm, DefaultItemWidthCm, DefaultItemHeightCm)];

        var actualWeightKg = packageItems.Sum(item => Math.Max(item.WeightKg ?? DefaultItemWeightKg, DefaultItemWeightKg) * item.Quantity);
        var volumeCm3 = packageItems.Sum(item => GetVolumeCm3(item) * item.Quantity);
        var volumetricWeightKg = volumeCm3 / VolumetricFactorCm3;
        var chargeableWeightKg = Math.Max(actualWeightKg, volumetricWeightKg);

        var includedWeightKg = destinationType == ShippingDestinationType.Lima ? 3m : 1m;
        var extraKgRate = destinationType == ShippingDestinationType.Lima ? 2m : 4m;
        var extraWeightKg = Math.Max(0, chargeableWeightKg - includedWeightKg);
        var weightSurcharge = Math.Ceiling(extraWeightKg) * extraKgRate;
        var bulkySurcharge = volumeCm3 > 45000m ? 8m : 0m;

        return new ShippingVolumePricing(
            decimal.Round(actualWeightKg, 2),
            decimal.Round(volumetricWeightKg, 2),
            decimal.Round(chargeableWeightKg, 2),
            weightSurcharge + bulkySurcharge);
    }

    public static ShippingVolumePricing CalculateForQuantity(ShippingDestinationType destinationType, int quantity)
        => Calculate(destinationType, quantity > 0
            ? [new ShippingPackageItem(quantity, null, null, null, null)]
            : null);

    private static decimal GetVolumeCm3(ShippingPackageItem item)
    {
        var length = Math.Max(item.LengthCm ?? DefaultItemLengthCm, 1m);
        var width = Math.Max(item.WidthCm ?? DefaultItemWidthCm, 1m);
        var height = Math.Max(item.HeightCm ?? DefaultItemHeightCm, 1m);
        return length * width * height;
    }
}

internal sealed class CreateShipmentCommandHandler(
    IRepository<ShipmentEntity> shipments,
    IRepository<OrderEntity> orders,
    IRepository<CourierEntity> couriers,
    IRepository<DepartmentEntity> departments,
    IRepository<Sweet_Silvia_Domain.Address.Address> addresses,
    IRepository<OlvaAgencyEntity> olvaAgencies,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateShipmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        if (order.Status is not OrderStatus.Paid and not OrderStatus.Preparing and not OrderStatus.Shipped)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipment.OrderNotReady",
                "A shipment can only be created for a paid order."));
        }

        var courier = await couriers.GetByIdAsync(request.CourierId, cancellationToken);
        if (courier is null || !courier.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Courier", request.CourierId));
        }

        DepartmentEntity? department;
        if (order.AddressId.HasValue)
        {
            var address = await addresses.GetByIdAsync(order.AddressId.Value, cancellationToken);
            if (address is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.NotFound("Address", order.AddressId.Value));
            }

            department = await departments.GetByIdAsync(address.DepartmentId, cancellationToken);
        }
        else
        {
            if (!order.ShippingAgencyId.HasValue)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict(
                    "Shipment.AgencyRequired",
                    "A province order without a delivery address must have a pickup agency."));
            }

            var agency = await olvaAgencies.GetByIdAsync(order.ShippingAgencyId.Value, cancellationToken);
            department = agency is null || !agency.IsActive || string.IsNullOrWhiteSpace(agency.Department)
                ? null
                : departments.Query().FirstOrDefault(item => item.IsActive && item.Name.ToLower() == agency.Department.Trim().ToLower());
        }

        if (department is null || !department.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Department"));
        }

        if (department.DestinationType == ShippingDestinationType.Province && courier.Type != CourierType.Agency)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipment.InvalidCourier",
                "Province orders can only be sent through an agency courier."));
        }

        if (shipments.Query().Any(shipment => shipment.OrderId == request.OrderId))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipment.AlreadyExists",
                "The order already has a shipment."));
        }

        if (request.ShippingCost != order.ShippingCost.Amount ||
            !string.Equals(request.Currency, order.ShippingCost.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipment.InvalidCost",
                "Shipment cost must match the shipping cost stored in the order."));
        }

        var shipment = ShipmentEntity.Create(request.OrderId, request.CourierId, Money.Create(request.ShippingCost, request.Currency), request.Observation);
        shipments.Add(shipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shipment.Id;
    }
}

internal sealed class RegisterExternalShipmentCommandHandler(IRepository<ShipmentEntity> shipments, IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterExternalShipmentCommand>
{
    public async Task<Result> Handle(RegisterExternalShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Shipment", request.ShipmentId));
        }

        shipment.RegisterExternalShipment(request.TrackingCode, request.ExternalShipmentCode, request.EstimatedDeliveryAt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ChangeShipmentStatusCommandHandler(
    IRepository<ShipmentEntity> shipments,
    IRepository<ShipmentEventEntity> events,
    IRepository<OrderEntity> orders,
    IRepository<OrderStatusHistoryEntity> orderHistories,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeShipmentStatusCommand>
{
    public async Task<Result> Handle(ChangeShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Shipment", request.ShipmentId));
        }

        var order = await orders.GetByIdAsync(shipment.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", shipment.OrderId));
        }

        if (request.ChangedByUserId == Guid.Empty)
        {
            return Result.Failure(ApplicationErrors.Conflict("Shipment.InvalidActor", "The user changing the shipment status is required."));
        }

        if (request.Status == ShipmentStatus.InTransit && order.Status != OrderStatus.Preparing)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Shipment.OrderNotPreparing",
                "The order must be in Preparing status before the shipment enters transit."));
        }

        if (request.Status == ShipmentStatus.Delivered && order.Status != OrderStatus.Shipped)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Shipment.OrderNotShipped",
                "The order must be Shipped before the shipment is delivered."));
        }

        shipment.ChangeStatus(request.Status, request.Observation);
        events.Add(ShipmentEventEntity.Create(request.ShipmentId, request.Status, request.Observation ?? $"Shipment status changed to {request.Status}.", null, DateTimeOffset.UtcNow));

        if (request.Status == ShipmentStatus.InTransit)
        {
            var previousStatus = order.Status;
            order.ChangeStatus(OrderStatus.Shipped, request.ChangedByUserId, request.Observation);
            orderHistories.Add(OrderStatusHistoryEntity.Create(order.Id, previousStatus, OrderStatus.Shipped, request.ChangedByUserId, request.Observation));
        }
        else if (request.Status == ShipmentStatus.Delivered)
        {
            var previousStatus = order.Status;
            order.ChangeStatus(OrderStatus.Delivered, request.ChangedByUserId, request.Observation);
            orderHistories.Add(OrderStatusHistoryEntity.Create(order.Id, previousStatus, OrderStatus.Delivered, request.ChangedByUserId, request.Observation));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetShipmentByIdQueryHandler(IRepository<ShipmentEntity> shipments)
    : IQueryHandler<GetShipmentByIdQuery, ShipmentResponse>
{
    public Task<Result<ShipmentResponse>> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var shipment = shipments.Query().FirstOrDefault(shipment => shipment.Id == request.Id);
        return Task.FromResult(shipment is null
            ? Result.Failure<ShipmentResponse>(ApplicationErrors.NotFound("Shipment", request.Id))
            : Result.Success(ShippingMappings.ToShipmentResponse(shipment)));
    }
}

internal sealed class GetShipmentsQueryHandler(IRepository<ShipmentEntity> shipments)
    : IQueryHandler<GetShipmentsQuery, List<ShipmentResponse>>
{
    public Task<Result<List<ShipmentResponse>>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var query = shipments.Query();
        if (request.Status.HasValue)
        {
            query = query.Where(shipment => shipment.Status == request.Status.Value);
        }

        return Task.FromResult(Result.Success(query.Select(ShippingMappings.ToShipmentResponse).ToList()));
    }
}

internal sealed class GetShipmentsByOrderQueryHandler(IRepository<ShipmentEntity> shipments)
    : IQueryHandler<GetShipmentsByOrderQuery, List<ShipmentResponse>>
{
    public Task<Result<List<ShipmentResponse>>> Handle(GetShipmentsByOrderQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(shipments.Query()
            .Where(shipment => shipment.OrderId == request.OrderId)
            .Select(ShippingMappings.ToShipmentResponse)
            .ToList()));
    }
}

internal sealed class CreateShipmentEventCommandHandler(
    IRepository<ShipmentEntity> shipments,
    IRepository<ShipmentEventEntity> events,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateShipmentEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateShipmentEventCommand request, CancellationToken cancellationToken)
    {
        var shipment = await shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Shipment", request.ShipmentId));
        }

        var shipmentEvent = ShipmentEventEntity.Create(request.ShipmentId, request.Status, request.Description, request.Location, request.EventDate);
        events.Add(shipmentEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shipmentEvent.Id;
    }
}

internal sealed class GetShipmentEventsQueryHandler(IRepository<ShipmentEventEntity> events)
    : IQueryHandler<GetShipmentEventsQuery, List<ShipmentEventResponse>>
{
    public Task<Result<List<ShipmentEventResponse>>> Handle(GetShipmentEventsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(events.Query()
            .Where(shipmentEvent => shipmentEvent.ShipmentId == request.ShipmentId)
            .OrderBy(shipmentEvent => shipmentEvent.EventDate)
            .Select(ShippingMappings.ToShipmentEventResponse)
            .ToList()));
    }
}

internal static class ShippingMappings
{
    public static Result ValidateShippingLocation(
        Guid departmentId,
        Guid? provinceId,
        Guid? districtId,
        ShippingDestinationType destinationType,
        IRepository<DepartmentEntity> departments,
        IRepository<ProvinceEntity> provinces,
        IRepository<DistrictEntity> districts)
    {
        var department = departments.Query().FirstOrDefault(department => department.Id == departmentId && department.IsActive);
        if (department is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Department", departmentId));
        }

        if (department.DestinationType != destinationType)
        {
            return Result.Failure(ApplicationErrors.Conflict("Shipping.InvalidDestination", "Department destination type does not match the selected shipping destination."));
        }

        if (provinceId.HasValue && !provinces.Query().Any(province => province.Id == provinceId.Value && province.DepartmentId == departmentId && province.IsActive))
        {
            return Result.Failure(ApplicationErrors.Conflict("Shipping.InvalidProvince", "Province does not belong to the selected department or is inactive."));
        }

        if (districtId.HasValue && (!provinceId.HasValue || !districts.Query().Any(district => district.Id == districtId.Value && district.ProvinceId == provinceId.Value && district.IsActive)))
        {
            return Result.Failure(ApplicationErrors.Conflict("Shipping.InvalidDistrict", "District does not belong to the selected province or is inactive."));
        }

        return Result.Success();
    }

    public static CourierResponse ToCourierResponse(CourierEntity courier)
    {
        return new CourierResponse(courier.Id, courier.Name, courier.Type, courier.RequiresExternalIntegration, courier.IsActive);
    }

    public static ShippingRateResponse ToRateResponse(ShippingRateEntity rate)
    {
        return new ShippingRateResponse(
            rate.Id,
            rate.CourierId,
            rate.CourierType,
            rate.DepartmentId,
            rate.ProvinceId,
            rate.DistrictId,
            rate.DestinationType,
            rate.ServiceType,
            rate.Cost.Amount,
            rate.Cost.Currency,
            rate.FreeShippingMinimumAmount?.Amount,
            rate.EstimatedTime,
            rate.IsActive);
    }

    public static ShipmentResponse ToShipmentResponse(ShipmentEntity shipment)
    {
        return new ShipmentResponse(
            shipment.Id,
            shipment.OrderId,
            shipment.CourierId,
            shipment.Status,
            shipment.TrackingCode,
            shipment.ExternalShipmentCode,
            shipment.ShippingCost.Amount,
            shipment.ShippingCost.Currency,
            shipment.CreatedAt,
            shipment.RegisteredExternallyAt,
            shipment.EstimatedDeliveryAt,
            shipment.DeliveredAt,
            shipment.Observation);
    }

    public static ShipmentEventResponse ToShipmentEventResponse(ShipmentEventEntity shipmentEvent)
    {
        return new ShipmentEventResponse(
            shipmentEvent.Id,
            shipmentEvent.ShipmentId,
            shipmentEvent.Status,
            shipmentEvent.Description,
            shipmentEvent.Location,
            shipmentEvent.EventDate);
    }
}
