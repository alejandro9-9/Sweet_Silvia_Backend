using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.ProductImage;

public sealed record ProductImageResponse(Guid Id, Guid ProductId, Guid? ProductVariantId, string Url, string? AltText, int Order, bool IsMain);

public sealed record CreateProductImageCommand(Guid ProductId, Guid? ProductVariantId, string Url, string? AltText, int Order, bool IsMain) : ICommand<Guid>;

public sealed record GetProductImagesByProductQuery(Guid ProductId) : IQuery<List<ProductImageResponse>>;
