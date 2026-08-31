using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Coupon;
using Sweet_Silvia_Application.CouponRestriction;
using Sweet_Silvia_Domain.Coupon;

namespace Sweet_Silvia_Api.Controllers.Coupons;

[ApiController]
[Route("api/coupons")]
public sealed class CouponsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetCouponsQuery(onlyActive), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponByCodeQuery(code), cancellationToken));

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate(ValidateCouponRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponByCodeQuery(request.Code), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCouponRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateCouponCommand(request.Code, request.Name, request.Scope, request.DiscountType, request.DiscountValue, request.StartDate, request.EndDate), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("{id:guid}/rules")]
    public async Task<IActionResult> ConfigureRules(Guid id, ConfigureCouponRulesRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ConfigureCouponRulesCommand(id, request.MinimumPurchaseAmount, request.MinimumItemsQuantity, request.MaximumDiscountedItemsQuantity, request.ApplicationStrategy, request.MaximumTotalUses, request.MaximumUsesPerUser), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteCouponCommand(id), cancellationToken));
}

[ApiController]
[Route("api/coupon-restrictions")]
public sealed class CouponRestrictionsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("coupon/{couponId:guid}")]
    public async Task<IActionResult> GetByCoupon(Guid couponId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouponRestrictionsQuery(couponId), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCouponRestrictionRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateCouponRestrictionCommand(request.CouponId, request.CategoryId, request.ProductId, request.ProductVariantId, request.CollectionId), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteCouponRestrictionCommand(id), cancellationToken));
}

public sealed record ValidateCouponRequest(string Code);
public sealed record CreateCouponRequest(string Code, string Name, CouponScope Scope, DiscountType DiscountType, decimal DiscountValue, DateTimeOffset StartDate, DateTimeOffset EndDate);
public sealed record ConfigureCouponRulesRequest(decimal? MinimumPurchaseAmount, int? MinimumItemsQuantity, int? MaximumDiscountedItemsQuantity, CouponApplicationStrategy? ApplicationStrategy, int? MaximumTotalUses, int? MaximumUsesPerUser);
public sealed record CreateCouponRestrictionRequest(Guid CouponId, Guid? CategoryId, Guid? ProductId, Guid? ProductVariantId, Guid? CollectionId);
