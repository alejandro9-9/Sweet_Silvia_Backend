using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Address;
using Sweet_Silvia_Application.Cart;
using Sweet_Silvia_Application.CartAppliedCoupon;
using Sweet_Silvia_Application.CartItem;
using Sweet_Silvia_Application.CouponUsage;
using Sweet_Silvia_Domain.Coupon;

namespace Sweet_Silvia_Api.Controllers.Customers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = ApiRoles.Customer)]
public sealed class CartController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetActiveCartByUserQuery(User.GetUserId()), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateCartCommand(User.GetUserId()), cancellationToken));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(request.CartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new AddCartItemCommand(request.CartId, request.ProductVariantId, request.Quantity), cancellationToken));
    }

    [HttpPut("items")]
    public async Task<IActionResult> UpdateItemQuantity(UpdateCartItemQuantityRequest request, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(request.CartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new UpdateCartItemQuantityCommand(request.CartId, request.ProductVariantId, request.Quantity), cancellationToken));
    }

    [HttpPatch("items/decrease")]
    public async Task<IActionResult> DecreaseItemQuantity(DecreaseCartItemQuantityRequest request, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(request.CartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new DecreaseCartItemQuantityCommand(request.CartId, request.ProductVariantId, request.Quantity), cancellationToken));
    }

    [HttpDelete("items")]
    public async Task<IActionResult> RemoveItem(RemoveCartItemRequest request, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(request.CartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new RemoveCartItemCommand(request.CartId, request.ProductVariantId), cancellationToken));
    }

    [HttpDelete("{cartId:guid}/items")]
    public async Task<IActionResult> Clear(Guid cartId, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(cartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new ClearCartCommand(cartId), cancellationToken));
    }

    [HttpGet("{cartId:guid}/items")]
    public async Task<IActionResult> GetItems(Guid cartId, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(cartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new GetCartItemsByCartQuery(cartId), cancellationToken));
    }

    [HttpPost("coupons")]
    public async Task<IActionResult> ApplyCoupon(ApplyCouponRequest request, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(request.CartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new ApplyCouponToCartCommand(request.CartId, User.GetUserId(), request.CouponId, request.Scope, request.CartItemId, request.AppliedQuantity), cancellationToken));
    }

    [HttpGet("{cartId:guid}/coupons")]
    public async Task<IActionResult> GetAppliedCoupons(Guid cartId, CancellationToken cancellationToken)
    {
        var ownership = await EnsureOwnsCart(cartId, cancellationToken);
        if (ownership is not null)
        {
            return ownership;
        }

        return this.ToActionResult(await sender.Send(new GetAppliedCouponsByCartQuery(cartId), cancellationToken));
    }

    [HttpDelete("coupons/{cartAppliedCouponId:guid}")]
    public async Task<IActionResult> RemoveCoupon(Guid cartAppliedCouponId, CancellationToken cancellationToken)
    {
        var activeCart = await sender.Send(new GetActiveCartByUserQuery(User.GetUserId()), cancellationToken);
        if (activeCart.IsFailure)
        {
            return this.ToActionResult(activeCart);
        }

        if (!activeCart.Value.AppliedCoupons.Any(coupon => coupon.Id == cartAppliedCouponId))
        {
            return Forbid();
        }

        return this.ToActionResult(await sender.Send(new RemoveCouponFromCartCommand(cartAppliedCouponId), cancellationToken));
    }

    private async Task<IActionResult?> EnsureOwnsCart(Guid cartId, CancellationToken cancellationToken)
    {
        var activeCart = await sender.Send(new GetActiveCartByUserQuery(User.GetUserId()), cancellationToken);
        if (activeCart.IsFailure)
        {
            return this.ToActionResult(activeCart);
        }

        return activeCart.Value.Id == cartId ? null : Forbid();
    }
}

[ApiController]
[Route("api/addresses")]
public sealed class AddressesController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.Customer)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetAddressesByUserQuery(User.GetUserId(), onlyActive), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetAddressesByUserQuery(userId, onlyActive), cancellationToken));

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAddressRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateAddressCommand(User.GetUserId(), request.ReceiverName, request.ReceiverPhone, request.DepartmentId, request.ProvinceId, request.DistrictId, request.Line, request.References, request.IsDefault), cancellationToken));

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        var address = await sender.Send(new GetAddressByIdQuery(id), cancellationToken);
        if (address.IsFailure)
        {
            return this.ToActionResult(address);
        }

        if (address.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return this.ToActionResult(await sender.Send(new UpdateAddressCommand(id, request.ReceiverName, request.ReceiverPhone, request.DepartmentId, request.ProvinceId, request.DistrictId, request.Line, request.References, request.IsDefault), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var address = await sender.Send(new GetAddressByIdQuery(id), cancellationToken);
        if (address.IsFailure)
        {
            return this.ToActionResult(address);
        }

        if (address.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return this.ToActionResult(await sender.Send(new DeleteAddressCommand(id), cancellationToken));
    }
}

[ApiController]
[Route("api/coupon-usages")]
public sealed class CouponUsagesController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponUsagesByUserQuery(userId), cancellationToken));

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponUsagesByUserQuery(User.GetUserId()), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet("coupon/{couponId:guid}")]
    public async Task<IActionResult> GetByCoupon(Guid couponId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponUsagesByCouponQuery(couponId), cancellationToken));
}

public sealed record AddCartItemRequest(Guid CartId, Guid ProductVariantId, int Quantity);
public sealed record UpdateCartItemQuantityRequest(Guid CartId, Guid ProductVariantId, int Quantity);
public sealed record DecreaseCartItemQuantityRequest(Guid CartId, Guid ProductVariantId, int Quantity);
public sealed record RemoveCartItemRequest(Guid CartId, Guid ProductVariantId);
public sealed record ApplyCouponRequest(Guid CartId, Guid CouponId, CouponScope Scope, Guid? CartItemId, int? AppliedQuantity);
public sealed record CreateAddressRequest(string ReceiverName, string ReceiverPhone, Guid DepartmentId, Guid ProvinceId, Guid DistrictId, string Line, string? References, bool IsDefault);
public sealed record UpdateAddressRequest(string ReceiverName, string ReceiverPhone, Guid DepartmentId, Guid ProvinceId, Guid DistrictId, string Line, string? References, bool IsDefault);
