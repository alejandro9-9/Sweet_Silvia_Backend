using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Product;

public sealed record ProductResponse(Guid Id, Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency, bool IsActive, int DisplayOrder);

public sealed record CreateProductCommand(Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency = "PEN") : ICommand<Guid>;

public sealed record UpdateProductCommand(Guid Id, Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency = "PEN") : ICommand;

public sealed record ReorderProductsCommand(IReadOnlyList<Guid> ProductIds) : ICommand;

public sealed record MoveProductToTopCommand(Guid Id) : ICommand;

public sealed record DeleteProductCommand(Guid Id) : ICommand;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;

public sealed record GetProductsQuery(Guid? CategoryId = null, Guid? CollectionId = null, bool OnlyActive = true) : IQuery<List<ProductResponse>>;
