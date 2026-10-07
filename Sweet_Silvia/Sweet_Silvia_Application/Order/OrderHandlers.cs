using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Coupons;
using Sweet_Silvia_Application.Abstractions.Orders;
using Sweet_Silvia_Application.Abstractions.Shipping;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.Courier;
using Sweet_Silvia_Application.Coupon;
using Sweet_Silvia_Application.Abstractions.Payments;
using Sweet_Silvia_Application.Izipay;
using Sweet_Silvia_Application.Olva;
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
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.PaymentMethod;
using Sweet_Silvia_Domain.ProductImage;
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
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;
using PaymentMethodEntity = Sweet_Silvia_Domain.PaymentMethod.PaymentMethod;
using ProductImageEntity = Sweet_Silvia_Domain.ProductImage.ProductImage;

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
    IRepository<PaymentEntity> payments,
    IRepository<PaymentMethodEntity> paymentMethods,
    IOlvaCourierClient olvaCourierClient,
    IIzipayPaymentGateway izipayPaymentGateway,
    IUserCheckoutLock userCheckoutLock,
    ICouponUsageLock couponUsageLock,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        const string storeCurrency = "PEN";

        if (request.IzipayPaymentMethodId.HasValue != (request.IzipayCustomer is not null))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Izipay.CheckoutDetailsRequired",
                "Izipay payment method and customer details must be provided together."));
        }

        PaymentMethodEntity? izipayPaymentMethod = null;
        if (request.IzipayPaymentMethodId.HasValue)
        {
            izipayPaymentMethod = await paymentMethods.GetByIdAsync(request.IzipayPaymentMethodId.Value, cancellationToken);
            if (izipayPaymentMethod is null || !izipayPaymentMethod.IsActive ||
                izipayPaymentMethod.Type != PaymentMethodType.Gateway || !izipayPaymentMethod.RequiresExternalIntegration)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                    "PaymentMethod.InvalidGateway",
                    "The selected payment method is not an active external gateway."));
            }

            var customerError = IzipayCustomerValidation.Validate(request.IzipayCustomer);
            if (customerError is not null)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Izipay.InvalidCustomer", customerError));
            }
        }

        await using var checkoutLock = izipayPaymentMethod is null
            ? null
            : await userCheckoutLock.AcquireAsync(request.UserId, cancellationToken);

        if (izipayPaymentMethod is not null)
        {
            var existingGatewayPayment = payments.Query()
                .Where(payment => payment.PaymentMethodId == izipayPaymentMethod.Id &&
                    payment.Status == PaymentStatus.PendingGateway &&
                    orders.Query().Any(order => order.Id == payment.OrderId &&
                        order.UserId == request.UserId &&
                        (order.Status == OrderStatus.PendingReceipt || order.Status == OrderStatus.ReceiptInReview)))
                .OrderByDescending(payment => payment.CreatedAt)
                .FirstOrDefault();

            if (existingGatewayPayment is not null)
            {
                var existingOrder = await orders.GetByIdAsync(existingGatewayPayment.OrderId, cancellationToken);
                var orderReference = existingGatewayPayment.GatewayTransactionId;
                if (existingOrder is null || string.IsNullOrWhiteSpace(orderReference))
                {
                    return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                        "Izipay.ActiveCheckoutInvalid",
                        "Existe un checkout Izipay activo que no se puede recuperar. Contacta a soporte antes de volver a intentar el pago."));
                }

                var gatewayResult = await izipayPaymentGateway.CreatePaymentFormAsync(
                    new IzipayPaymentFormGatewayRequest(
                        orderReference,
                        existingOrder.Total.Amount,
                        existingOrder.Total.Currency,
                        request.IzipayCustomer!),
                    cancellationToken);
                if (gatewayResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponse>(gatewayResult.Error);
                }

                existingGatewayPayment.RefreshGatewaySession(DateTimeOffset.UtcNow.AddHours(24), "FORM_TOKEN_REFRESHED");
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var existingForm = new IzipayPaymentFormResponse(
                    existingGatewayPayment.Id,
                    existingOrder.Id,
                    orderReference,
                    gatewayResult.Value.FormToken,
                    gatewayResult.Value.PublicKey,
                    decimal.ToInt64(existingOrder.Total.Amount * 100m),
                    existingOrder.Total.Currency);
                return new CreateOrderResponse(existingOrder.Id, existingOrder.Total.Amount, existingOrder.Total.Currency, existingForm);
            }
        }

        var usesLiveOlvaQuote = !request.ShippingRateId.HasValue;
        var shippingRate = request.ShippingRateId.HasValue
            ? await shippingRates.GetByIdAsync(request.ShippingRateId.Value, cancellationToken)
            : null;
        if ((!usesLiveOlvaQuote && (shippingRate is null || !shippingRate.IsActive)) ||
            (usesLiveOlvaQuote && (!request.ShippingAgencyId.HasValue || request.ExpectedOlvaShippingCost is not > 0)))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.InvalidRate",
                "Selecciona una tarifa vigente o cotiza el envio Olva antes de crear el pedido."));
        }

        var address = request.AddressId.HasValue
            ? await addresses.GetByIdAsync(request.AddressId.Value, cancellationToken)
            : null;
        if (request.AddressId.HasValue && (address is null || !address.IsActive || address.UserId != request.UserId))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.NotFound("Address", request.AddressId.Value));
        }

        var isDistrictSpecificOwnDelivery = !usesLiveOlvaQuote &&
            shippingRate!.CourierType == CourierType.OwnDelivery &&
            shippingRate.DistrictId.HasValue;
        if (isDistrictSpecificOwnDelivery && (!request.AddressId.HasValue || request.ShippingAgencyId.HasValue))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.AddressRequired",
                "El reparto propio requiere una dirección de entrega y no admite recojo en agencia."));
        }

        if (!request.AddressId.HasValue && !usesLiveOlvaQuote && shippingRate!.DestinationType != ShippingDestinationType.Province)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.AddressRequired",
                "A delivery address is required for this shipping service."));
        }

        var departmentId = address?.DepartmentId ?? request.ShippingDepartmentId;
        var provinceId = address?.ProvinceId ?? request.ShippingProvinceId;
        var districtId = address?.DistrictId ?? request.ShippingDistrictId;
        if (!departmentId.HasValue || !provinceId.HasValue || !districtId.HasValue)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.LocationRequired",
                "Department, province and district are required to create the order."));
        }

        var department = departments.Query()
            .FirstOrDefault(item => item.Id == departmentId.Value && item.IsActive);
        if (department is null)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.NotFound("Department", departmentId.Value));
        }

        var courier = usesLiveOlvaQuote
            ? couriers.Query().FirstOrDefault(item => item.IsActive && item.Type == CourierType.Agency && item.Name.ToLower().Contains("olva"))
            : await couriers.GetByIdAsync(shippingRate!.CourierId, cancellationToken);
        if (courier is null || !courier.IsActive)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.CourierUnavailable",
                "The selected shipping service is currently unavailable."));
        }

        if (!usesLiveOlvaQuote && (shippingRate!.DestinationType != department.DestinationType ||
            !shippingRate.AppliesTo(departmentId.Value, provinceId.Value, districtId.Value, department.DestinationType)))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.InvalidRate",
                "The selected shipping rate does not apply to the address."));
        }

        var province = provinces.Query()
            .FirstOrDefault(item => item.Id == provinceId.Value && item.IsActive);
        var district = districts.Query()
            .FirstOrDefault(item => item.Id == districtId.Value && item.IsActive);

        if (province is null || district is null)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.NotFound("ShippingLocation"));
        }

        var isOlvaProvinceDestination = IsOlvaProvinceDestination(department, province);
        if (usesLiveOlvaQuote && !isOlvaProvinceDestination)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.InvalidDestination",
                "La cotización de Olva solo está disponible para destinos fuera de Lima Metropolitana."));
        }

        if (usesLiveOlvaQuote && shippingRates.Query().Any(rate =>
            rate.IsActive &&
            rate.CourierType == CourierType.OwnDelivery &&
            rate.DepartmentId == department.Id &&
            rate.ProvinceId == province.Id &&
            rate.DistrictId == district.Id))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.LocalDeliveryRequired",
                "Este distrito tiene reparto propio y no puede enviarse por Olva."));
        }

        if ((!usesLiveOlvaQuote && courier.Type != shippingRate!.CourierType) ||
            (isOlvaProvinceDestination && courier.Type != CourierType.Agency && !isDistrictSpecificOwnDelivery))
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.InvalidCourier",
                "El servicio de envío seleccionado no es compatible con el destino."));
        }

        if (isOlvaProvinceDestination && !isDistrictSpecificOwnDelivery && !request.ShippingAgencyId.HasValue)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Shipping.AgencyRequired",
                "A pickup agency is required for province shipments."));
        }

        if (request.ShippingAgencyId.HasValue)
        {
            var shippingAgency = await olvaAgencies.GetByIdAsync(request.ShippingAgencyId.Value, cancellationToken);
            if (shippingAgency is null || !shippingAgency.IsActive ||
                !OlvaLocationNames.Matches(shippingAgency.Department, department.Name) ||
                !OlvaLocationNames.Matches(shippingAgency.Province, province.Name) ||
                !OlvaLocationNames.Matches(shippingAgency.District, district.Name))
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                    "Shipping.InvalidAgency",
                    "The selected agency is not available for the delivery address."));
            }
        }

        var cart = carts.Query().FirstOrDefault(item => item.UserId == request.UserId && item.IsActive);
        if (cart is null)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.NotFound("Cart"));
        }

        var currentCartItems = cartItems.Query()
            .Where(item => item.CartId == cart.Id)
            .ToList();
        if (currentCartItems.Count == 0)
        {
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Order.EmptyCart", "The cart does not contain items."));
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
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                    "Order.InvalidCartItem",
                    "The cart contains an inactive or unavailable product."));
            }

            if (variant.PhysicalStock < cartItem.Quantity)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                    "Stock.NotEnough",
                    $"Product variant '{variant.Id}' does not have enough stock."));
            }

            var unitPrice = variant.Price ?? product.BasePrice;
            if (unitPrice.Currency != storeCurrency)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
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
            return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Coupon.NotAppliedToCart",
                "The selected coupon is not applied to the current cart."));
        }

        var selectedCartCoupons = request.CouponId.HasValue
            ? cartCoupons.Where(item => item.CouponId == request.CouponId.Value).ToList()
            : cartCoupons;
        if (selectedCartCoupons.Count > 1)
        {
        return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                "Coupon.MultipleCouponsNotSupported",
                "Only one coupon can be applied to an order."));
        }

        var selectedCoupons = new List<(CartAppliedCouponEntity Applied, CouponEntity Coupon, List<CouponRestrictionEntity> Restrictions)>();

        foreach (var cartCoupon in selectedCartCoupons)
        {
            var coupon = await coupons.GetByIdAsync(cartCoupon.CouponId, cancellationToken);
            if (coupon is null || !coupon.IsActive || DateTimeOffset.UtcNow < coupon.StartDate || DateTimeOffset.UtcNow > coupon.EndDate)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Coupon.Invalid", "A coupon applied to the cart is no longer valid."));
            }

            var exceededLimit = CouponUsageLimitGuard.GetExceededLimit(
                coupon,
                request.UserId,
                orders.Query(),
                couponUsages.Query());
            if (exceededLimit == CouponUsageLimitExceeded.Total)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Coupon.TotalUsesExceeded", "A coupon has reached its total usage limit."));
            }

            if (exceededLimit == CouponUsageLimitExceeded.PerUser)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Coupon.UserUsesExceeded", "A coupon has already been used by this user."));
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
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                    "Coupon.NotApplicable",
                    "The selected coupon does not apply to the current cart."));
            }
        }

        var subtotal = Money.Create(lines.Sum(item => item.UnitPrice * item.Quantity), storeCurrency);
        var discountTotal = Money.Create(lines.Sum(item => item.DiscountAmount), storeCurrency);
        decimal shippingAmount;
        if (usesLiveOlvaQuote)
        {
            var package = OlvaQuotePricing.CalculatePackage(lines.Select(line => OlvaQuotePricing.ToPackageItem(
                line.Quantity,
                line.Product.Name,
                line.Variant.ShippingWeightKg,
                line.Variant.ShippingLengthCm,
                line.Variant.ShippingWidthCm,
                line.Variant.ShippingHeightCm)).ToList());
            var quote = await olvaCourierClient.QuoteAsync(
                new OlvaQuoteRequest(
                    OlvaQuotePricing.CheckoutOriginUbigeo,
                    district.Ubigeo ?? string.Empty,
                    "O",
                    2,
                    package.ActualWeightKg,
                    OlvaQuotePricing.CheckoutPartnerRate,
                    package.LengthCm,
                    package.WidthCm,
                    package.HeightCm),
                cancellationToken);
            if (quote.IsFailure)
            {
                return Result.Failure<CreateOrderResponse>(quote.Error);
            }

            if (!OlvaQuotePricing.TryReadCost(quote.Value.Data, out shippingAmount))
            {
                return Result.Failure<CreateOrderResponse>(new Error(
                    "Olva.QuotePriceMissing",
                    "Olva respondio sin un precio de cotizacion reconocible."));
            }

            if (Math.Abs(shippingAmount - request.ExpectedOlvaShippingCost!.Value) > 0.01m)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict(
                    "Shipping.QuoteChanged",
                    "La tarifa de Olva cambio. Actualiza la cotizacion antes de confirmar el pedido."));
            }
        }
        else
        {
            var baseShippingCost = shippingRate!.CalculateCost(subtotal);
            var volumePricing = ShippingVolumeCalculator.Calculate(
                shippingRate.CourierType == CourierType.OwnDelivery
                    ? ShippingDestinationType.Lima
                    : department.DestinationType,
                lines.Select(line => new ShippingPackageItem(
                    line.Quantity,
                    line.Variant.ShippingWeightKg,
                    line.Variant.ShippingLengthCm,
                    line.Variant.ShippingWidthCm,
                    line.Variant.ShippingHeightCm)).ToList());
            shippingAmount = baseShippingCost.Amount + volumePricing.Surcharge;
        }

        var shippingCost = Money.Create(shippingAmount, storeCurrency);
        var total = Money.Create(subtotal.Amount - discountTotal.Amount + shippingCost.Amount, storeCurrency);

        IzipayPaymentFormGatewayResponse? izipayForm = null;
        string? izipayOrderReference = null;
        if (izipayPaymentMethod is not null)
        {
            izipayOrderReference = Guid.NewGuid().ToString("N");
            var gatewayResult = await izipayPaymentGateway.CreatePaymentFormAsync(
                new IzipayPaymentFormGatewayRequest(
                    izipayOrderReference,
                    total.Amount,
                    total.Currency,
                    request.IzipayCustomer!),
                cancellationToken);

            if (gatewayResult.IsFailure)
            {
                return Result.Failure<CreateOrderResponse>(gatewayResult.Error);
            }

            izipayForm = gatewayResult.Value;
        }

        await using var couponLock = selectedCoupons.Count == 0
            ? null
            : await couponUsageLock.AcquireAsync(selectedCoupons[0].Coupon.Id, cancellationToken);
        foreach (var selectedCoupon in selectedCoupons)
        {
            var exceededLimit = CouponUsageLimitGuard.GetExceededLimit(
                selectedCoupon.Coupon,
                request.UserId,
                orders.Query(),
                couponUsages.Query());
            if (exceededLimit == CouponUsageLimitExceeded.Total)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Coupon.TotalUsesExceeded", "A coupon has reached its total usage limit."));
            }

            if (exceededLimit == CouponUsageLimitExceeded.PerUser)
            {
                return Result.Failure<CreateOrderResponse>(ApplicationErrors.Conflict("Coupon.UserUsesExceeded", "A coupon has already been used by this user."));
            }
        }

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

        PaymentEntity? izipayPayment = null;
        if (izipayPaymentMethod is not null && izipayOrderReference is not null)
        {
            izipayPayment = PaymentEntity.CreateGatewayPending(
                order.Id,
                izipayPaymentMethod.Id,
                total,
                izipayOrderReference,
                izipayOrderReference,
                DateTimeOffset.UtcNow.AddHours(24),
                "FORM_TOKEN_CREATED");
            payments.Add(izipayPayment);
        }

        if (izipayPayment is null)
        {
            foreach (var cartItem in currentCartItems)
            {
                cartItems.Remove(cartItem);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var formResponse = izipayForm is null || izipayPayment is null || izipayOrderReference is null
            ? null
            : new IzipayPaymentFormResponse(
                izipayPayment.Id,
                order.Id,
                izipayOrderReference,
                izipayForm.FormToken,
                izipayForm.PublicKey,
                decimal.ToInt64(total.Amount * 100m),
                total.Currency);

        return new CreateOrderResponse(order.Id, total.Amount, total.Currency, formResponse);
    }

    private static bool IsOlvaProvinceDestination(DepartmentEntity department, ProvinceEntity province)
    {
        if (department.DestinationType == ShippingDestinationType.Province)
        {
            return true;
        }

        return OlvaLocationNames.Normalize(department.Name) == "LIMA" &&
            OlvaLocationNames.Normalize(province.Name) is not "LIMA" and not "LIMA METROPOLITANA";
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

internal sealed class GetOrderItemsByOrderQueryHandler(
    IRepository<OrderItemEntity> orderItems,
    IRepository<ProductVariantEntity> variants,
    IRepository<ProductImageEntity> productImages)
    : IQueryHandler<GetOrderItemsByOrderQuery, List<OrderItemResponse>>
{
    public Task<Result<List<OrderItemResponse>>> Handle(GetOrderItemsByOrderQuery request, CancellationToken cancellationToken)
    {
        var orderItemsForOrder = orderItems.Query()
            .Where(item => item.OrderId == request.OrderId)
            .ToList();
        var variantIds = orderItemsForOrder.Select(item => item.ProductVariantId).Distinct().ToHashSet();
        var variantsById = variants.Query()
            .Where(variant => variantIds.Contains(variant.Id))
            .ToDictionary(variant => variant.Id);
        var productIds = variantsById.Values.Select(variant => variant.ProductId).Distinct().ToHashSet();
        var imagesByProduct = productImages.Query()
            .Where(image => productIds.Contains(image.ProductId))
            .OrderBy(image => image.Order)
            .ToList()
            .GroupBy(image => image.ProductId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var response = orderItemsForOrder.Select(item =>
        {
            variantsById.TryGetValue(item.ProductVariantId, out var variant);
            imagesByProduct.TryGetValue(variant?.ProductId ?? Guid.Empty, out var images);
            var imageUrl = images?
                .OrderByDescending(image => image.ProductVariantId == item.ProductVariantId)
                .ThenByDescending(image => image.IsMain)
                .ThenBy(image => image.Order)
                .FirstOrDefault()?.Url;

            return OrderMappings.ToItemResponse(item, imageUrl);
        }).ToList();

        return Task.FromResult(Result.Success(response));
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

    public static OrderItemResponse ToItemResponse(OrderItemEntity item, string? imageUrl = null)
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
            item.Subtotal.Currency,
            imageUrl);
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
