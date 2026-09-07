using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.CouponRestriction;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponRestrictionEntity = Sweet_Silvia_Domain.CouponRestriction.CouponRestriction;

namespace Sweet_Silvia_Application.Coupon;

internal sealed class CreateCouponCommandHandler(IRepository<CouponEntity> coupons, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCouponCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var code = CouponCode.Create(request.Code);
        if (coupons.Query().Any(coupon => coupon.Code == code))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("Coupon.CodeAlreadyExists", "Coupon code is already registered."));
        }

        var coupon = CouponEntity.Create(code, request.Name, request.Scope, request.DiscountType, request.DiscountValue, request.StartDate, request.EndDate);
        coupons.Add(coupon);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }
}

internal sealed class ConfigureCouponRulesCommandHandler(IRepository<CouponEntity> coupons, IUnitOfWork unitOfWork)
    : ICommandHandler<ConfigureCouponRulesCommand>
{
    public async Task<Result> Handle(ConfigureCouponRulesCommand request, CancellationToken cancellationToken)
    {
        var coupon = await coupons.GetByIdAsync(request.Id, cancellationToken);
        if (coupon is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Coupon", request.Id));
        }

        coupon.ConfigureRules(
            request.MinimumPurchaseAmount,
            request.MinimumItemsQuantity,
            request.MaximumDiscountedItemsQuantity,
            request.ApplicationStrategy,
            request.MaximumTotalUses,
            request.MaximumUsesPerUser);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteCouponCommandHandler(IRepository<CouponEntity> coupons, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCouponCommand>
{
    public async Task<Result> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await coupons.GetByIdAsync(request.Id, cancellationToken);
        if (coupon is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Coupon", request.Id));
        }

        coupon.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetCouponByIdQueryHandler(IRepository<CouponEntity> coupons)
    : IQueryHandler<GetCouponByIdQuery, CouponResponse>
{
    public Task<Result<CouponResponse>> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = coupons.Query().FirstOrDefault(coupon => coupon.Id == request.Id);
        return Task.FromResult(coupon is null
            ? Result.Failure<CouponResponse>(ApplicationErrors.NotFound("Coupon", request.Id))
            : Result.Success(CouponMappings.ToResponse(coupon)));
    }
}

internal sealed class GetCouponByCodeQueryHandler(IRepository<CouponEntity> coupons)
    : IQueryHandler<GetCouponByCodeQuery, CouponResponse>
{
    public Task<Result<CouponResponse>> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var code = CouponCode.Create(request.Code);
        var now = DateTimeOffset.UtcNow;
        var coupon = coupons.Query().FirstOrDefault(coupon =>
            coupon.Code == code &&
            coupon.IsActive &&
            coupon.StartDate <= now &&
            coupon.EndDate >= now);
        return Task.FromResult(coupon is null
            ? Result.Failure<CouponResponse>(ApplicationErrors.NotFound("Coupon"))
            : Result.Success(CouponMappings.ToResponse(coupon)));
    }
}

internal sealed class GetCouponsQueryHandler(IRepository<CouponEntity> coupons)
    : IQueryHandler<GetCouponsQuery, List<CouponResponse>>
{
    public Task<Result<List<CouponResponse>>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = coupons.Query();
        if (request.OnlyActive)
        {
            query = query.Where(coupon => coupon.IsActive);
        }

        return Task.FromResult(Result.Success(query.Select(CouponMappings.ToResponse).ToList()));
    }
}

internal sealed class CreateCouponRestrictionCommandHandler(
    IRepository<CouponEntity> coupons,
    IRepository<CouponRestrictionEntity> restrictions,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCouponRestrictionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCouponRestrictionCommand request, CancellationToken cancellationToken)
    {
        var coupon = await coupons.GetByIdAsync(request.CouponId, cancellationToken);
        if (coupon is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Coupon", request.CouponId));
        }

        var restriction = CouponRestrictionEntity.Create(
            request.CouponId,
            request.CategoryId,
            request.ProductId,
            request.ProductVariantId,
            request.CollectionId);

        restrictions.Add(restriction);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return restriction.Id;
    }
}

internal sealed class DeleteCouponRestrictionCommandHandler(IRepository<CouponRestrictionEntity> restrictions, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCouponRestrictionCommand>
{
    public async Task<Result> Handle(DeleteCouponRestrictionCommand request, CancellationToken cancellationToken)
    {
        var restriction = await restrictions.GetByIdAsync(request.Id, cancellationToken);
        if (restriction is not null)
        {
            restrictions.Remove(restriction);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}

internal sealed class GetCouponRestrictionsQueryHandler(IRepository<CouponRestrictionEntity> restrictions)
    : IQueryHandler<GetCouponRestrictionsQuery, List<CouponRestrictionResponse>>
{
    public Task<Result<List<CouponRestrictionResponse>>> Handle(GetCouponRestrictionsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(restrictions.Query()
            .Where(restriction => restriction.CouponId == request.CouponId)
            .Select(CouponMappings.ToRestrictionResponse)
            .ToList()));
    }
}

internal static class CouponMappings
{
    public static CouponResponse ToResponse(CouponEntity coupon)
    {
        return new CouponResponse(
            coupon.Id,
            coupon.Code.Value,
            coupon.Name,
            coupon.Scope,
            coupon.DiscountType,
            coupon.DiscountValue,
            coupon.MinimumPurchaseAmount,
            coupon.MinimumItemsQuantity,
            coupon.MaximumDiscountedItemsQuantity,
            coupon.ApplicationStrategy,
            coupon.StartDate,
            coupon.EndDate,
            coupon.IsActive,
            coupon.MaximumTotalUses,
            coupon.MaximumUsesPerUser);
    }

    public static CouponRestrictionResponse ToRestrictionResponse(CouponRestrictionEntity restriction)
    {
        return new CouponRestrictionResponse(
            restriction.Id,
            restriction.CouponId,
            restriction.CategoryId,
            restriction.ProductId,
            restriction.ProductVariantId,
            restriction.CollectionId);
    }
}
