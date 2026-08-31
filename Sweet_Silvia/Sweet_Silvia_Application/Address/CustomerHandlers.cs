using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Cart;
using Sweet_Silvia_Application.CartAppliedCoupon;
using Sweet_Silvia_Application.CartItem;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.CouponUsage;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Coupon;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using AddressEntity = Sweet_Silvia_Domain.Address.Address;
using CartEntity = Sweet_Silvia_Domain.Cart.Cart;
using CartAppliedCouponEntity = Sweet_Silvia_Domain.CartAppliedCoupon.CartAppliedCoupon;
using CartItemEntity = Sweet_Silvia_Domain.CartItem.CartItem;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponRestrictionEntity = Sweet_Silvia_Domain.CouponRestriction.CouponRestriction;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;
using DepartmentEntity = Sweet_Silvia_Domain.Department.Department;
using DistrictEntity = Sweet_Silvia_Domain.District.District;
using ProductEntity = Sweet_Silvia_Domain.Product.Product;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using ProvinceEntity = Sweet_Silvia_Domain.Province.Province;

namespace Sweet_Silvia_Application.Address;

internal sealed class CreateAddressCommandHandler(
    IRepository<AddressEntity> addresses,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAddressCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var validation = ValidateLocation(request.DepartmentId, request.ProvinceId, request.DistrictId, departments, provinces, districts);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Error);
        }

        if (request.IsDefault)
        {
            UnsetOtherDefaultAddresses(request.UserId, addresses);
        }

        var address = AddressEntity.Create(
            request.UserId,
            request.ReceiverName,
            PhoneNumber.Create(request.ReceiverPhone),
            request.DepartmentId,
            request.ProvinceId,
            request.DistrictId,
            AddressLine.Create(request.Line),
            request.References,
            request.IsDefault);

        addresses.Add(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return address.Id;
    }

    internal static Result ValidateLocation(
        Guid departmentId,
        Guid provinceId,
        Guid districtId,
        IRepository<DepartmentEntity> departments,
        IRepository<ProvinceEntity> provinces,
        IRepository<DistrictEntity> districts)
    {
        var department = departments.Query().FirstOrDefault(department => department.Id == departmentId && department.IsActive);
        if (department is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Department", departmentId));
        }

        var province = provinces.Query().FirstOrDefault(province =>
            province.Id == provinceId &&
            province.DepartmentId == departmentId &&
            province.IsActive);
        if (province is null)
        {
            return Result.Failure(ApplicationErrors.Conflict("Address.InvalidProvince", "Province does not belong to the selected department or is inactive."));
        }

        var district = districts.Query().FirstOrDefault(district =>
            district.Id == districtId &&
            district.ProvinceId == provinceId &&
            district.IsActive);
        if (district is null)
        {
            return Result.Failure(ApplicationErrors.Conflict("Address.InvalidDistrict", "District does not belong to the selected province or is inactive."));
        }

        return Result.Success();
    }

    internal static void UnsetOtherDefaultAddresses(Guid userId, IRepository<AddressEntity> addresses)
    {
        foreach (var address in addresses.Query().Where(address => address.UserId == userId && address.IsDefault && address.IsActive))
        {
            address.Update(
                address.ReceiverName,
                address.ReceiverPhone,
                address.DepartmentId,
                address.ProvinceId,
                address.DistrictId,
                address.Line,
                address.References,
                false);
        }
    }
}

internal sealed class UpdateAddressCommandHandler(
    IRepository<AddressEntity> addresses,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAddressCommand>
{
    public async Task<Result> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await addresses.GetByIdAsync(request.Id, cancellationToken);
        if (address is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Address", request.Id));
        }

        var validation = CreateAddressCommandHandler.ValidateLocation(request.DepartmentId, request.ProvinceId, request.DistrictId, departments, provinces, districts);
        if (validation.IsFailure)
        {
            return validation;
        }

        if (request.IsDefault)
        {
            CreateAddressCommandHandler.UnsetOtherDefaultAddresses(address.UserId, addresses);
        }

        address.Update(
            request.ReceiverName,
            PhoneNumber.Create(request.ReceiverPhone),
            request.DepartmentId,
            request.ProvinceId,
            request.DistrictId,
            AddressLine.Create(request.Line),
            request.References,
            request.IsDefault);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteAddressCommandHandler(IRepository<AddressEntity> addresses, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAddressCommand>
{
    public async Task<Result> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await addresses.GetByIdAsync(request.Id, cancellationToken);
        if (address is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Address", request.Id));
        }

        address.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetAddressByIdQueryHandler(IRepository<AddressEntity> addresses)
    : IQueryHandler<GetAddressByIdQuery, AddressResponse>
{
    public Task<Result<AddressResponse>> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var address = addresses.Query().FirstOrDefault(address => address.Id == request.Id);
        return Task.FromResult(address is null
            ? Result.Failure<AddressResponse>(ApplicationErrors.NotFound("Address", request.Id))
            : Result.Success(CustomerMappings.ToResponse(address)));
    }
}

internal sealed class GetAddressesByUserQueryHandler(IRepository<AddressEntity> addresses)
    : IQueryHandler<GetAddressesByUserQuery, List<AddressResponse>>
{
    public Task<Result<List<AddressResponse>>> Handle(GetAddressesByUserQuery request, CancellationToken cancellationToken)
    {
        var query = addresses.Query().Where(address => address.UserId == request.UserId);
        if (request.OnlyActive)
        {
            query = query.Where(address => address.IsActive);
        }

        return Task.FromResult(Result.Success(query.Select(CustomerMappings.ToResponse).ToList()));
    }
}

internal sealed class CreateCartCommandHandler(IRepository<CartEntity> carts, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCartCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var activeCart = carts.Query().FirstOrDefault(cart => cart.UserId == request.UserId && cart.IsActive);
        if (activeCart is not null)
        {
            return activeCart.Id;
        }

        var cart = CartEntity.Create(request.UserId);
        carts.Add(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cart.Id;
    }
}

internal sealed class GetActiveCartByUserQueryHandler(
    IRepository<CartEntity> carts,
    IRepository<CartItemEntity> cartItems,
    IRepository<CartAppliedCouponEntity> appliedCoupons)
    : IQueryHandler<GetActiveCartByUserQuery, CartResponse>
{
    public Task<Result<CartResponse>> Handle(GetActiveCartByUserQuery request, CancellationToken cancellationToken)
    {
        var cart = carts.Query().FirstOrDefault(cart => cart.UserId == request.UserId && cart.IsActive);
        return Task.FromResult(cart is null
            ? Result.Failure<CartResponse>(ApplicationErrors.NotFound("Cart"))
            : Result.Success(CustomerMappings.ToCartResponse(cart, cartItems, appliedCoupons)));
    }
}

internal sealed class AddCartItemCommandHandler(
    IRepository<CartEntity> carts,
    IRepository<CartItemEntity> cartItems,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddCartItemCommand>
{
    public async Task<Result> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await carts.GetByIdAsync(request.CartId, cancellationToken);
        if (cart is null || !cart.IsActive)
        {
            return Result.Failure(ApplicationErrors.NotFound("Cart", request.CartId));
        }

        var item = cartItems.Query().FirstOrDefault(item => item.CartId == request.CartId && item.ProductVariantId == request.ProductVariantId);
        if (item is null)
        {
            cartItems.Add(CartItemEntity.Create(request.CartId, request.ProductVariantId, request.Quantity));
        }
        else
        {
            item.IncreaseQuantity(request.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class UpdateCartItemQuantityCommandHandler(IRepository<CartItemEntity> cartItems, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = cartItems.Query().FirstOrDefault(item => item.CartId == request.CartId && item.ProductVariantId == request.ProductVariantId);
        if (item is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("CartItem"));
        }

        if (request.Quantity <= 0)
        {
            cartItems.Remove(item);
        }
        else
        {
            item.SetQuantity(request.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DecreaseCartItemQuantityCommandHandler(IRepository<CartItemEntity> cartItems, IUnitOfWork unitOfWork)
    : ICommandHandler<DecreaseCartItemQuantityCommand>
{
    public async Task<Result> Handle(DecreaseCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = cartItems.Query().FirstOrDefault(item => item.CartId == request.CartId && item.ProductVariantId == request.ProductVariantId);
        if (item is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("CartItem"));
        }

        if (request.Quantity >= item.Quantity)
        {
            cartItems.Remove(item);
        }
        else
        {
            item.DecreaseQuantity(request.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class RemoveCartItemCommandHandler(IRepository<CartItemEntity> cartItems, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveCartItemCommand>
{
    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var item = cartItems.Query().FirstOrDefault(item => item.CartId == request.CartId && item.ProductVariantId == request.ProductVariantId);
        if (item is not null)
        {
            cartItems.Remove(item);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}

internal sealed class ClearCartCommandHandler(IRepository<CartItemEntity> cartItems, IUnitOfWork unitOfWork)
    : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in cartItems.Query().Where(item => item.CartId == request.CartId))
        {
            cartItems.Remove(item);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeactivateCartCommandHandler(IRepository<CartEntity> carts, IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateCartCommand>
{
    public async Task<Result> Handle(DeactivateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await carts.GetByIdAsync(request.CartId, cancellationToken);
        if (cart is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Cart", request.CartId));
        }

        cart.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetCartItemsByCartQueryHandler(IRepository<CartItemEntity> cartItems)
    : IQueryHandler<GetCartItemsByCartQuery, List<CartItemResponse>>
{
    public Task<Result<List<CartItemResponse>>> Handle(GetCartItemsByCartQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(cartItems.Query()
            .Where(item => item.CartId == request.CartId)
            .Select(item => new CartItemResponse(item.Id, item.CartId, item.ProductVariantId, item.Quantity, item.AddedAt, item.UpdatedAt))
            .ToList()));
    }
}

internal sealed class ApplyCouponToCartCommandHandler(
    IRepository<CartEntity> carts,
    IRepository<CartItemEntity> cartItems,
    IRepository<CartAppliedCouponEntity> appliedCoupons,
    IRepository<CouponEntity> coupons,
    IRepository<CouponRestrictionEntity> restrictions,
    IRepository<CouponUsageEntity> couponUsages,
    IRepository<ProductVariantEntity> variants,
    IRepository<ProductEntity> products,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ApplyCouponToCartCommand, CartAppliedCouponResponse>
{
    public async Task<Result<CartAppliedCouponResponse>> Handle(ApplyCouponToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await carts.GetByIdAsync(request.CartId, cancellationToken);
        if (cart is null || !cart.IsActive)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.NotFound("Cart", request.CartId));
        }

        var coupon = await coupons.GetByIdAsync(request.CouponId, cancellationToken);
        if (coupon is null || !coupon.IsActive || DateTimeOffset.UtcNow < coupon.StartDate || DateTimeOffset.UtcNow > coupon.EndDate)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.Invalid", "Coupon is not active or is outside its valid date range."));
        }

        if (coupon.Scope != request.Scope)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.InvalidScope", "Coupon scope does not match the requested application."));
        }

        if (coupon.MaximumTotalUses.HasValue && couponUsages.Query().Count(usage => usage.CouponId == coupon.Id) >= coupon.MaximumTotalUses.Value)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.TotalUsesExceeded", "Coupon has reached its total usage limit."));
        }

        if (coupon.MaximumUsesPerUser.HasValue && couponUsages.Query().Count(usage => usage.CouponId == coupon.Id && usage.UserId == request.UserId) >= coupon.MaximumUsesPerUser.Value)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.UserUsesExceeded", "Coupon has already been used by this user."));
        }

        if (appliedCoupons.Query().Any(appliedCoupon => appliedCoupon.CartId == request.CartId && appliedCoupon.CouponId == request.CouponId))
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.AlreadyApplied", "Coupon is already applied to this cart."));
        }

        if (coupon.Scope == CouponScope.Order && appliedCoupons.Query().Any(appliedCoupon => appliedCoupon.CartId == request.CartId && appliedCoupon.Scope == CouponScope.Order))
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.OrderCouponAlreadyApplied", "Cart already has an order coupon applied."));
        }

        if (coupon.Scope == CouponScope.Item && !request.CartItemId.HasValue)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.CartItemRequired", "Item coupons require a cart item."));
        }

        var discount = CalculateEstimatedDiscount(request, coupon, cartItems, restrictions, variants, products);
        if (discount.Amount <= 0)
        {
            return Result.Failure<CartAppliedCouponResponse>(ApplicationErrors.Conflict("Coupon.NotApplicable", "Coupon does not apply to the current cart."));
        }

        var appliedCoupon = CartAppliedCouponEntity.Create(
            request.CartId,
            request.CouponId,
            coupon.Scope,
            request.CartItemId,
            request.AppliedQuantity,
            discount);

        appliedCoupons.Add(appliedCoupon);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerMappings.ToAppliedCouponResponse(appliedCoupon);
    }

    private static Money CalculateEstimatedDiscount(
        ApplyCouponToCartCommand request,
        CouponEntity coupon,
        IRepository<CartItemEntity> cartItems,
        IRepository<CouponRestrictionEntity> restrictions,
        IRepository<ProductVariantEntity> variants,
        IRepository<ProductEntity> products)
    {
        var cartItemQuery = cartItems.Query().Where(item => item.CartId == request.CartId);
        if (coupon.Scope == CouponScope.Item)
        {
            cartItemQuery = cartItemQuery.Where(item => item.Id == request.CartItemId);
        }

        var restrictionList = restrictions.Query().Where(restriction => restriction.CouponId == coupon.Id).ToList();
        var eligibleAmounts = new List<decimal>();

        foreach (var item in cartItemQuery.ToList())
        {
            var variant = variants.Query().FirstOrDefault(variant => variant.Id == item.ProductVariantId && variant.IsActive);
            if (variant is null)
            {
                continue;
            }

            var product = products.Query().FirstOrDefault(product => product.Id == variant.ProductId && product.IsActive);
            if (product is null || !MatchesRestrictions(product, variant, restrictionList))
            {
                continue;
            }

            var unitPrice = variant.Price ?? product.BasePrice;
            var quantity = coupon.Scope == CouponScope.Item
                ? Math.Min(request.AppliedQuantity ?? item.Quantity, item.Quantity)
                : item.Quantity;
            eligibleAmounts.AddRange(Enumerable.Repeat(unitPrice.Amount, quantity));
        }

        if (coupon.MinimumItemsQuantity.HasValue && eligibleAmounts.Count < coupon.MinimumItemsQuantity.Value)
        {
            return Money.Zero();
        }

        var eligibleSubtotal = eligibleAmounts.Sum();
        if (coupon.MinimumPurchaseAmount.HasValue && eligibleSubtotal < coupon.MinimumPurchaseAmount.Value)
        {
            return Money.Zero();
        }

        if (coupon.MaximumDiscountedItemsQuantity.HasValue)
        {
            eligibleAmounts = coupon.ApplicationStrategy switch
            {
                CouponApplicationStrategy.MostExpensive => eligibleAmounts.OrderDescending().Take(coupon.MaximumDiscountedItemsQuantity.Value).ToList(),
                CouponApplicationStrategy.Cheapest => eligibleAmounts.Order().Take(coupon.MaximumDiscountedItemsQuantity.Value).ToList(),
                _ => eligibleAmounts.Take(coupon.MaximumDiscountedItemsQuantity.Value).ToList()
            };
            eligibleSubtotal = eligibleAmounts.Sum();
        }

        var amount = coupon.DiscountType == DiscountType.Percentage
            ? eligibleSubtotal * coupon.DiscountValue / 100m
            : coupon.Scope == CouponScope.Item
                ? Math.Min(eligibleSubtotal, coupon.DiscountValue * Math.Max(1, eligibleAmounts.Count))
                : Math.Min(eligibleSubtotal, coupon.DiscountValue);

        return Money.Create(amount);
    }

    private static bool MatchesRestrictions(ProductEntity product, ProductVariantEntity variant, List<CouponRestrictionEntity> restrictions)
    {
        return restrictions.Count == 0 || restrictions.Any(restriction =>
            restriction.CategoryId == product.CategoryId ||
            restriction.ProductId == product.Id ||
            restriction.ProductVariantId == variant.Id ||
            restriction.CollectionId == product.CollectionId);
    }
}

internal sealed class RemoveCouponFromCartCommandHandler(IRepository<CartAppliedCouponEntity> appliedCoupons, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveCouponFromCartCommand>
{
    public async Task<Result> Handle(RemoveCouponFromCartCommand request, CancellationToken cancellationToken)
    {
        var appliedCoupon = await appliedCoupons.GetByIdAsync(request.CartAppliedCouponId, cancellationToken);
        if (appliedCoupon is not null)
        {
            appliedCoupons.Remove(appliedCoupon);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}

internal sealed class GetAppliedCouponsByCartQueryHandler(IRepository<CartAppliedCouponEntity> appliedCoupons)
    : IQueryHandler<GetAppliedCouponsByCartQuery, List<CartAppliedCouponResponse>>
{
    public Task<Result<List<CartAppliedCouponResponse>>> Handle(GetAppliedCouponsByCartQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(appliedCoupons.Query()
            .Where(coupon => coupon.CartId == request.CartId)
            .Select(CustomerMappings.ToAppliedCouponResponse)
            .ToList()));
    }
}

internal sealed class RegisterCouponUsageCommandHandler(IRepository<CouponUsageEntity> usages, IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterCouponUsageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterCouponUsageCommand request, CancellationToken cancellationToken)
    {
        var usage = CouponUsageEntity.Create(
            request.CouponId,
            request.UserId,
            request.OrderId,
            request.Scope,
            request.OrderItemId,
            request.AppliedQuantity,
            Money.Create(request.DiscountAmount, request.Currency));

        usages.Add(usage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return usage.Id;
    }
}

internal sealed class GetCouponUsagesByCouponQueryHandler(IRepository<CouponUsageEntity> usages)
    : IQueryHandler<GetCouponUsagesByCouponQuery, List<CouponUsageResponse>>
{
    public Task<Result<List<CouponUsageResponse>>> Handle(GetCouponUsagesByCouponQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(usages.Query()
            .Where(usage => usage.CouponId == request.CouponId)
            .Select(CustomerMappings.ToCouponUsageResponse)
            .ToList()));
    }
}

internal sealed class GetCouponUsagesByUserQueryHandler(IRepository<CouponUsageEntity> usages)
    : IQueryHandler<GetCouponUsagesByUserQuery, List<CouponUsageResponse>>
{
    public Task<Result<List<CouponUsageResponse>>> Handle(GetCouponUsagesByUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(usages.Query()
            .Where(usage => usage.UserId == request.UserId)
            .Select(CustomerMappings.ToCouponUsageResponse)
            .ToList()));
    }
}

internal static class CustomerMappings
{
    public static AddressResponse ToResponse(AddressEntity address)
    {
        return new AddressResponse(
            address.Id,
            address.UserId,
            address.ReceiverName,
            address.ReceiverPhone.Value,
            address.Country,
            address.DepartmentId,
            address.ProvinceId,
            address.DistrictId,
            address.Line.Value,
            address.References,
            address.IsDefault,
            address.IsActive);
    }

    public static CartResponse ToCartResponse(CartEntity cart, IRepository<CartItemEntity> cartItems, IRepository<CartAppliedCouponEntity> appliedCoupons)
    {
        return new CartResponse(
            cart.Id,
            cart.UserId,
            cart.IsActive,
            cart.CreatedAt,
            cart.UpdatedAt,
            cartItems.Query()
                .Where(item => item.CartId == cart.Id)
                .Select(item => new CartItemSummaryResponse(item.Id, item.ProductVariantId, item.Quantity))
                .ToList(),
            appliedCoupons.Query()
                .Where(coupon => coupon.CartId == cart.Id)
                .Select(coupon => new CartAppliedCouponSummaryResponse(
                    coupon.Id,
                    coupon.CouponId,
                    coupon.CartItemId,
                    coupon.AppliedQuantity,
                    coupon.EstimatedDiscountAmount.Amount,
                    coupon.EstimatedDiscountAmount.Currency))
                .ToList());
    }

    public static CartAppliedCouponResponse ToAppliedCouponResponse(CartAppliedCouponEntity coupon)
    {
        return new CartAppliedCouponResponse(
            coupon.Id,
            coupon.CartId,
            coupon.CouponId,
            coupon.Scope,
            coupon.CartItemId,
            coupon.AppliedQuantity,
            coupon.EstimatedDiscountAmount.Amount,
            coupon.EstimatedDiscountAmount.Currency,
            coupon.AppliedAt);
    }

    public static CouponUsageResponse ToCouponUsageResponse(CouponUsageEntity usage)
    {
        return new CouponUsageResponse(
            usage.Id,
            usage.CouponId,
            usage.UserId,
            usage.OrderId,
            usage.Scope,
            usage.OrderItemId,
            usage.AppliedQuantity,
            usage.DiscountAmount.Amount,
            usage.DiscountAmount.Currency,
            usage.UsedAt);
    }
}
