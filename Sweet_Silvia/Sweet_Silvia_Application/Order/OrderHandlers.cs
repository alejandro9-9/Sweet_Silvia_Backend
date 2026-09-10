using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.Courier;
using Sweet_Silvia_Application.OrderItem;
using Sweet_Silvia_Application.OrderStatusHistory;
using Sweet_Silvia_Application.ShippingRate;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.CartAppliedCoupon;
using Sweet_Silvia_Domain.CartItem;
using Sweet_Silvia_Domain.Coupon;
using Sweet_Silvia_Domain.Courier;
using Sweet_Silvia_Domain.District;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.OlvaAgency;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using Sweet_Silvia_Domain.StockMovement;
using AddressEntity = Sweet_Silvia_Domain.Address.Address;
using CartAppliedCouponEntity = Sweet_Silvia_Domain.CartAppliedCoupon.CartAppliedCoupon;
using CartEntity = Sweet_Silvia_Domain.Cart.Cart;
using CartItemEntity = Sweet_Silvia_Domain.CartItem.CartItem;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponRestrictionEntity = Sweet_Silvia_Domain.CouponRestriction.CouponRestriction;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;
using DepartmentEntity = Sweet_Silvia_Domain.Department.Department;
using DistrictEntity = Sweet_Silvia_Domain.District.District;
using CourierEntity = Sweet_Silvia_Domain.Courier.Courier;
using ProductEntity = Sweet_Silvia_Domain.Product.Product;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using ShippingRateEntity = Sweet_Silvia_Domain.ShippingRate.ShippingRate;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using OrderStatusHistoryEntity = Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory;
using OlvaAgencyEntity = Sweet_Silvia_Domain.OlvaAgency.OlvaAgency;
using ProvinceEntity = Sweet_Silvia_Domain.Province.Province;
using StockMovementEntity = Sweet_Silvia_Domain.StockMovement.StockMovement;

namespace Sweet_Silvia_Application.Order;

internal sealed class CreateOrderCommandHandler(
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IRepository<AddressEntity> addresses,
    IRepository<CartEntity> carts,
    IRepository<CartItemEntity> cartItems,
    IRepository<CartAppliedCouponEntity> appliedCoupons,
    IRepository<CouponEntity> coupons,
    IRepository<CouponRestrictionEntity> restrictions,
    IRepository<CouponUsageEntity> couponUsages,
    IRepository<ProductEntity> products,
    IRepository<ProductVariantEntity> variants,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts,
    IRepository<CourierEntity> couriers,
    IRepository<ShippingRateEntity> shippingRates,
    IRepository<OlvaAgencyEntity> olvaAgencies,
    IRepository<StockMovementEntity> stockMovements,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        const string storeCurrency = "PEN";

        var address = await addresses.GetByIdAsync(request.AddressId, cancellationToken);
        if (address is null || !address.IsActive || address.UserId != request.UserId)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Address", request.AddressId));
        }

        var department = departments.Query()
            .FirstOrDefault(item => item.Id == address.DepartmentId && item.IsActive);
        if (department is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Department", address.DepartmentId));
        }

        var shippingRate = await shippingRates.GetByIdAsync(request.ShippingRateId, cancellationToken);
        if (shippingRate is null || !shippingRate.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("ShippingRate", request.ShippingRateId));
        }

        var courier = await couriers.GetByIdAsync(shippingRate.CourierId, cancellationToken);
        if (courier is null || !courier.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipping.CourierUnavailable",
                "The selected shipping service is currently unavailable."));
        }

        if (courier.Type != shippingRate.CourierType ||
            (department.DestinationType == ShippingDestinationType.Province && courier.Type != CourierType.Agency))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipping.InvalidCourier",
                "The selected shipping service is not compatible with the destination."));
        }

        if (shippingRate.DestinationType != department.DestinationType ||
            !shippingRate.AppliesTo(address.DepartmentId, address.ProvinceId, address.DistrictId, department.DestinationType))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipping.InvalidRate",
                "The selected shipping rate does not apply to the address."));
        }

        var province = provinces.Query()
            .FirstOrDefault(item => item.Id == address.ProvinceId && item.IsActive);
        var district = districts.Query()
            .FirstOrDefault(item => item.Id == address.DistrictId && item.IsActive);

        if (province is null || district is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("ShippingLocation"));
        }

        if (department.DestinationType == ShippingDestinationType.Province && !request.ShippingAgencyId.HasValue)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Shipping.AgencyRequired",
                "A pickup agency is required for province shipments."));
        }

        if (request.ShippingAgencyId.HasValue)
        {
            var shippingAgency = await olvaAgencies.GetByIdAsync(request.ShippingAgencyId.Value, cancellationToken);
            if (shippingAgency is null || !shippingAgency.IsActive ||
                !string.Equals(shippingAgency.Department, department.Name, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(shippingAgency.Province, province.Name, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(shippingAgency.District) &&
                 !string.Equals(shippingAgency.District, district.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict(
                    "Shipping.InvalidAgency",
                    "The selected agency is not available for the delivery address."));
            }
        }

        var cart = carts.Query().FirstOrDefault(item => item.UserId == request.UserId && item.IsActive);
        if (cart is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Cart"));
        }

        var currentCartItems = cartItems.Query()
            .Where(item => item.CartId == cart.Id)
            .ToList();
        if (currentCartItems.Count == 0)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("Order.EmptyCart", "The cart does not contain items."));
        }

        var variantIds = currentCartItems.Select(item => item.ProductVariantId).ToList();
        var currentVariants = variants.Query()
            .Where(item => variantIds.Contains(item.Id))
            .ToDictionary(item => item.Id);
        var productIds = currentVariants.Values.Select(item => item.ProductId).Distinct().ToList();
        var currentProducts = products.Query()
            .Where(item => productIds.Contains(item.Id))
            .ToDictionary(item => item.Id);
        var lines = new List<CheckoutLine>();

        foreach (var cartItem in currentCartItems)
        {
            if (!currentVariants.TryGetValue(cartItem.ProductVariantId, out var variant) ||
                !variant.IsActive ||
                !currentProducts.TryGetValue(variant.ProductId, out var product) ||
                !product.IsActive)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict(
                    "Order.InvalidCartItem",
                    "The cart contains an inactive or unavailable product."));
            }

            if (variant.PhysicalStock < cartItem.Quantity)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict(
                    "Stock.NotEnough",
                    $"Product variant '{variant.Id}' does not have enough stock."));
            }

            var unitPrice = variant.Price ?? product.BasePrice;
            if (unitPrice.Currency != storeCurrency)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict(
                    "Order.UnsupportedCurrency",
                    $"All store products must use {storeCurrency}."));
            }

            lines.Add(new CheckoutLine(cartItem, variant, product, unitPrice.Amount));
        }

        var cartCoupons = appliedCoupons.Query()
            .Where(item => item.CartId == cart.Id)
            .ToList();
        if (request.CouponId.HasValue && !cartCoupons.Any(item => item.CouponId == request.CouponId.Value))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Coupon.NotAppliedToCart",
                "The selected coupon is not applied to the current cart."));
        }

        var selectedCartCoupons = request.CouponId.HasValue
            ? cartCoupons.Where(item => item.CouponId == request.CouponId.Value).ToList()
            : cartCoupons;
        if (selectedCartCoupons.Count > 1)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Coupon.MultipleCouponsNotSupported",
                "Only one coupon can be applied to an order."));
        }

        var selectedCoupons = new List<(CartAppliedCouponEntity Applied, CouponEntity Coupon, List<CouponRestrictionEntity> Restrictions)>();

        foreach (var cartCoupon in selectedCartCoupons)
        {
            var coupon = await coupons.GetByIdAsync(cartCoupon.CouponId, cancellationToken);
            if (coupon is null || !coupon.IsActive || DateTimeOffset.UtcNow < coupon.StartDate || DateTimeOffset.UtcNow > coupon.EndDate)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict("Coupon.Invalid", "A coupon applied to the cart is no longer valid."));
            }

            if (coupon.MaximumTotalUses.HasValue &&
                couponUsages.Query().Count(item => item.CouponId == coupon.Id) >= coupon.MaximumTotalUses.Value)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict("Coupon.TotalUsesExceeded", "A coupon has reached its total usage limit."));
            }

            if (coupon.MaximumUsesPerUser.HasValue &&
                couponUsages.Query().Count(item => item.CouponId == coupon.Id && item.UserId == request.UserId) >= coupon.MaximumUsesPerUser.Value)
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict("Coupon.UserUsesExceeded", "A coupon has already been used by this user."));
            }

            selectedCoupons.Add((
                cartCoupon,
                coupon,
                restrictions.Query().Where(item => item.CouponId == coupon.Id).ToList()));
        }

        foreach (var selectedCoupon in selectedCoupons)
        {
            if (!ApplyCoupon(selectedCoupon.Applied, selectedCoupon.Coupon, selectedCoupon.Restrictions, lines))
            {
                return Result.Failure<Guid>(ApplicationErrors.Conflict(
                    "Coupon.NotApplicable",
                    "The selected coupon does not apply to the current cart."));
            }
        }

        var subtotal = Money.Create(lines.Sum(item => item.UnitPrice * item.Quantity), storeCurrency);
        var discountTotal = Money.Create(lines.Sum(item => item.DiscountAmount), storeCurrency);
        var baseShippingCost = shippingRate.CalculateCost(subtotal);
        var totalQuantity = lines.Sum(item => item.Quantity);
        var volumePricing = ShippingVolumeCalculator.Calculate(
            department.DestinationType,
            lines.Select(line => new ShippingPackageItem(
                line.Quantity,
                line.Variant.ShippingWeightKg,
                line.Variant.ShippingLengthCm,
                line.Variant.ShippingWidthCm,
                line.Variant.ShippingHeightCm)).ToList());
        var shippingCost = Money.Create(baseShippingCost.Amount + volumePricing.Surcharge, storeCurrency);
        var total = Money.Create(subtotal.Amount - discountTotal.Amount + shippingCost.Amount, storeCurrency);

        var order = OrderEntity.Create(
            request.UserId,
            request.AddressId,
            request.ShippingAgencyId,
            selectedCoupons.FirstOrDefault(item => item.Coupon.Scope == CouponScope.Order).Coupon?.Id ?? selectedCoupons.FirstOrDefault().Coupon?.Id,
            selectedCoupons.FirstOrDefault().Applied?.AppliedQuantity,
            subtotal,
            discountTotal,
            shippingCost,
            total);

        orders.Add(order);
        foreach (var line in lines)
        {
            var previousStock = line.Variant.PhysicalStock;
            line.Variant.DecreaseStock(line.Quantity);
            stockMovements.Add(StockMovementEntity.Create(
                line.Variant.Id,
                order.Id,
                StockMovementType.Reservation,
                line.Quantity,
                previousStock,
                line.Variant.PhysicalStock,
                "Stock reserved while the customer completes payment."));

            AddOrderItemSnapshot(order, line, line.DiscountedQuantity, line.DiscountAmount, orderItems, storeCurrency);
            AddOrderItemSnapshot(order, line, line.Quantity - line.DiscountedQuantity, 0m, orderItems, storeCurrency);
        }

        foreach (var cartItem in currentCartItems)
        {
            cartItems.Remove(cartItem);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.Id;
    }

    private static bool ApplyCoupon(
        CartAppliedCouponEntity appliedCoupon,
        CouponEntity coupon,
        List<CouponRestrictionEntity> couponRestrictions,
        List<CheckoutLine> lines)
    {
        var candidates = lines
            .SelectMany((line, index) =>
            {
                if (coupon.Scope == CouponScope.Item && line.CartItem.Id != appliedCoupon.CartItemId)
                {
                    return [];
                }

                var quantity = coupon.Scope == CouponScope.Item
                    ? Math.Min(appliedCoupon.AppliedQuantity ?? line.Quantity, line.Quantity)
                    : line.Quantity;

                return MatchesRestrictions(line, couponRestrictions)
                    ? Enumerable.Repeat(new EligibleUnit(index, line.UnitPrice), quantity)
                    : [];
            })
            .ToList();

        if (coupon.MinimumItemsQuantity.HasValue && candidates.Count < coupon.MinimumItemsQuantity.Value)
        {
            return false;
        }

        var eligibleSubtotal = candidates.Sum(item => item.UnitPrice);
        if (coupon.MinimumPurchaseAmount.HasValue && eligibleSubtotal < coupon.MinimumPurchaseAmount.Value)
        {
            return false;
        }

        var selected = coupon.MaximumDiscountedItemsQuantity.HasValue
            ? coupon.ApplicationStrategy switch
            {
                CouponApplicationStrategy.MostExpensive => candidates.OrderByDescending(item => item.UnitPrice).Take(coupon.MaximumDiscountedItemsQuantity.Value).ToList(),
                CouponApplicationStrategy.Cheapest => candidates.OrderBy(item => item.UnitPrice).Take(coupon.MaximumDiscountedItemsQuantity.Value).ToList(),
                _ => candidates.Take(coupon.MaximumDiscountedItemsQuantity.Value).ToList()
            }
            : candidates;

        var selectedSubtotal = selected.Sum(item => item.UnitPrice);
        if (selectedSubtotal <= 0)
        {
            return false;
        }

        var discount = coupon.DiscountType == DiscountType.Percentage
            ? selectedSubtotal * coupon.DiscountValue / 100m
            : coupon.Scope == CouponScope.Item
                ? coupon.DiscountValue * selected.Count
                : coupon.DiscountValue;
        discount = Math.Min(discount, selectedSubtotal);

        var selectedByLine = selected
            .GroupBy(item => item.LineIndex)
            .Select(group => (LineIndex: group.Key, Amount: group.Sum(item => item.UnitPrice), Quantity: group.Count()))
            .ToList();
        var remainingDiscount = discount;
        for (var index = 0; index < selectedByLine.Count; index++)
        {
            var allocation = index == selectedByLine.Count - 1
                ? remainingDiscount
                : discount * selectedByLine[index].Amount / selectedSubtotal;
            var line = lines[selectedByLine[index].LineIndex];
            var maximumAdditionalDiscount = line.UnitPrice * line.Quantity - line.DiscountAmount;
            var appliedAmount = Math.Min(Math.Max(allocation, 0m), maximumAdditionalDiscount);
            line.DiscountAmount += appliedAmount;
            line.DiscountedQuantity += selectedByLine[index].Quantity;
            remainingDiscount -= appliedAmount;
        }

        return discount > 0;
    }

    private static void AddOrderItemSnapshot(
        OrderEntity order,
        CheckoutLine line,
        int quantity,
        decimal discountAmount,
        IRepository<OrderItemEntity> orderItems,
        string currency)
    {
        if (quantity <= 0)
        {
            return;
        }

        var unitDiscount = Money.Create(Math.Min(line.UnitPrice, discountAmount / quantity), currency);
        orderItems.Add(OrderItemEntity.Create(
            order.Id,
            line.Variant.Id,
            line.Product.Name,
            line.Variant.Sku.Value,
            line.Variant.Size,
            line.Variant.Color,
            quantity,
            Money.Create(line.UnitPrice, currency),
            unitDiscount,
            Money.Create((line.UnitPrice - unitDiscount.Amount) * quantity, currency)));
    }

    private static bool MatchesRestrictions(CheckoutLine line, List<CouponRestrictionEntity> restrictions)
    {
        return restrictions.Count == 0 || restrictions.Any(restriction =>
            restriction.CategoryId == line.Product.CategoryId ||
            restriction.ProductId == line.Product.Id ||
            restriction.ProductVariantId == line.Variant.Id ||
            restriction.CollectionId == line.Product.CollectionId);
    }

    private sealed class CheckoutLine(
        CartItemEntity cartItem,
        ProductVariantEntity variant,
        ProductEntity product,
        decimal unitPrice)
    {
        public CartItemEntity CartItem { get; } = cartItem;
        public ProductVariantEntity Variant { get; } = variant;
        public ProductEntity Product { get; } = product;
        public decimal UnitPrice { get; } = unitPrice;
        public int Quantity => CartItem.Quantity;
        public decimal DiscountAmount { get; set; }
        public int DiscountedQuantity { get; set; }
    }

    private sealed record EligibleUnit(int LineIndex, decimal UnitPrice);
}

internal sealed class AddOrderItemCommandHandler(
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddOrderItemCommand>
{
    public async Task<Result> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        var item = OrderItemEntity.Create(
            request.OrderId,
            request.ProductVariantId,
            request.ProductName,
            request.Sku,
            request.Size,
            request.Color,
            request.Quantity,
            Money.Create(request.UnitPrice, request.Currency),
            Money.Create(request.UnitDiscount, request.Currency),
            Money.Create(request.Subtotal, request.Currency));

        orderItems.Add(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ChangeOrderStatusCommandHandler(
    IRepository<OrderEntity> orders,
    IRepository<OrderStatusHistoryEntity> histories,
    IRepository<ProductVariantEntity> variants,
    IRepository<StockMovementEntity> stockMovements,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeOrderStatusCommand>
{
    public async Task<Result> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.Status == OrderStatus.Paid)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Order.PaymentConfirmationRequired",
                "An order can only be marked as paid by the payment confirmation flow."));
        }

        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        if ((request.Status is OrderStatus.Cancelled or OrderStatus.OutOfStock) &&
            order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Order.InvalidCancellation",
                "Only unpaid orders can be cancelled from the operations panel."));
        }

        if (request.Status is OrderStatus.Cancelled or OrderStatus.OutOfStock)
        {
            var reservations = stockMovements.Query()
                .Where(movement => movement.OrderId == order.Id && movement.Type == StockMovementType.Reservation)
                .ToList();

            foreach (var reservation in reservations)
            {
                var variant = await variants.GetByIdAsync(reservation.ProductVariantId, cancellationToken);
                if (variant is null)
                {
                    return Result.Failure(ApplicationErrors.NotFound("ProductVariant", reservation.ProductVariantId));
                }

                var previousStock = variant.PhysicalStock;
                variant.IncreaseStock(reservation.Quantity);
                stockMovements.Add(StockMovementEntity.Create(
                    variant.Id,
                    order.Id,
                    StockMovementType.Cancellation,
                    reservation.Quantity,
                    previousStock,
                    variant.PhysicalStock,
                    "Stock reservation released because the order was cancelled."));
            }
        }

        var previousStatus = order.Status;
        order.ChangeStatus(request.Status, request.ChangedByUserId, request.Observation);
        histories.Add(OrderStatusHistoryEntity.Create(request.OrderId, previousStatus, request.Status, request.ChangedByUserId, request.Observation));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetOrderByIdQueryHandler(IRepository<OrderEntity> orders)
    : IQueryHandler<GetOrderByIdQuery, OrderResponse>
{
    public Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = orders.Query().FirstOrDefault(order => order.Id == request.Id);
        return Task.FromResult(order is null
            ? Result.Failure<OrderResponse>(ApplicationErrors.NotFound("Order", request.Id))
            : Result.Success(OrderMappings.ToResponse(order)));
    }
}

internal sealed class GetOrdersQueryHandler(IRepository<OrderEntity> orders)
    : IQueryHandler<GetOrdersQuery, List<OrderResponse>>
{
    public Task<Result<List<OrderResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orders.Query();
        if (request.Status.HasValue)
        {
            query = query.Where(order => order.Status == request.Status.Value);
        }

        return Task.FromResult(Result.Success(query.Select(OrderMappings.ToResponse).ToList()));
    }
}

internal sealed class GetOrdersByUserQueryHandler(IRepository<OrderEntity> orders)
    : IQueryHandler<GetOrdersByUserQuery, List<OrderResponse>>
{
    public Task<Result<List<OrderResponse>>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(orders.Query()
            .Where(order => order.UserId == request.UserId)
            .Select(OrderMappings.ToResponse)
            .ToList()));
    }
}

internal sealed class GetOrderItemsByOrderQueryHandler(IRepository<OrderItemEntity> orderItems)
    : IQueryHandler<GetOrderItemsByOrderQuery, List<OrderItemResponse>>
{
    public Task<Result<List<OrderItemResponse>>> Handle(GetOrderItemsByOrderQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(orderItems.Query()
            .Where(item => item.OrderId == request.OrderId)
            .Select(OrderMappings.ToItemResponse)
            .ToList()));
    }
}

internal sealed class GetOrderStatusHistoryQueryHandler(IRepository<OrderStatusHistoryEntity> histories)
    : IQueryHandler<GetOrderStatusHistoryQuery, List<OrderStatusHistoryResponse>>
{
    public Task<Result<List<OrderStatusHistoryResponse>>> Handle(GetOrderStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(histories.Query()
            .Where(history => history.OrderId == request.OrderId)
            .OrderBy(history => history.ChangedAt)
            .Select(OrderMappings.ToHistoryResponse)
            .ToList()));
    }
}

internal static class OrderMappings
{
    public static OrderResponse ToResponse(OrderEntity order)
    {
        return new OrderResponse(
            order.Id,
            order.UserId,
            order.AddressId,
            order.ShippingAgencyId,
            order.CouponId,
            order.CouponAppliedQuantity,
            order.Status,
            order.Subtotal.Amount,
            order.DiscountTotal.Amount,
            order.ShippingCost.Amount,
            order.Total.Amount,
            order.Total.Currency,
            order.CreatedAt,
            order.UpdatedAt);
    }

    public static OrderItemResponse ToItemResponse(OrderItemEntity item)
    {
        return new OrderItemResponse(
            item.Id,
            item.OrderId,
            item.ProductVariantId,
            item.HistoricalProductName,
            item.HistoricalSku,
            item.HistoricalSize,
            item.HistoricalColor,
            item.Quantity,
            item.HistoricalUnitPrice.Amount,
            item.HistoricalUnitDiscount.Amount,
            item.Subtotal.Amount,
            item.Subtotal.Currency);
    }

    public static OrderStatusHistoryResponse ToHistoryResponse(OrderStatusHistoryEntity history)
    {
        return new OrderStatusHistoryResponse(
            history.Id,
            history.OrderId,
            history.PreviousStatus,
            history.NewStatus,
            history.ChangedByUserId,
            history.ChangedAt,
            history.Observation);
    }
}
