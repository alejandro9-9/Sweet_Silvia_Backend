using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.ProductVariant;

public sealed record ProductVariantResponse(Guid Id, Guid ProductId, string Size, string Color, string Sku, int PhysicalStock, decimal? Price, string? Currency, decimal ShippingWeightKg, decimal ShippingLengthCm, decimal ShippingWidthCm, decimal ShippingHeightCm, bool IsActive);

public sealed record CreateProductVariantCommand(Guid ProductId, string Size, string Color, string Sku, int PhysicalStock, decimal? Price = null, string Currency = "PEN", decimal ShippingWeightKg = 0.3m, decimal ShippingLengthCm = 30m, decimal ShippingWidthCm = 22m, decimal ShippingHeightCm = 4m) : ICommand<Guid>;

public sealed record UpdateProductVariantCommand(Guid Id, string Size, string Color, decimal? Price = null, string Currency = "PEN", decimal? ShippingWeightKg = null, decimal? ShippingLengthCm = null, decimal? ShippingWidthCm = null, decimal? ShippingHeightCm = null) : ICommand;

public sealed record DeleteProductVariantCommand(Guid Id) : ICommand;

public sealed record IncreaseProductVariantStockCommand(Guid Id, int Quantity) : ICommand;

public sealed record DecreaseProductVariantStockCommand(Guid Id, int Quantity) : ICommand;

public sealed record GetProductVariantByIdQuery(Guid Id) : IQuery<ProductVariantResponse>;

public sealed record GetProductVariantsByProductQuery(Guid ProductId, bool OnlyActive = true) : IQuery<List<ProductVariantResponse>>;
