using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.CouponRestriction;

public sealed record CouponRestrictionResponse(Guid Id, Guid CouponId, Guid? CategoryId, Guid? ProductId, Guid? ProductVariantId, Guid? CollectionId);

public sealed record CreateCouponRestrictionCommand(Guid CouponId, Guid? CategoryId = null, Guid? ProductId = null, Guid? ProductVariantId = null, Guid? CollectionId = null) : ICommand<Guid>;

public sealed record GetCouponRestrictionsQuery(Guid CouponId) : IQuery<List<CouponRestrictionResponse>>;
