using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.StockMovement;

namespace Sweet_Silvia_Application.StockMovement;

public sealed record StockMovementResponse(Guid Id, Guid ProductVariantId, Guid? OrderId, StockMovementType Type, int Quantity, int PreviousStock, int NewStock, DateTimeOffset CreatedAt, string? Observation);

public sealed record CreateStockMovementCommand(Guid ProductVariantId, Guid? OrderId, StockMovementType Type, int Quantity, int PreviousStock, int NewStock, string? Observation) : ICommand<Guid>;

public sealed record GetStockMovementsQuery() : IQuery<List<StockMovementResponse>>;

public sealed record GetStockMovementsByProductVariantQuery(Guid ProductVariantId) : IQuery<List<StockMovementResponse>>;

public sealed record GetStockMovementsByOrderQuery(Guid OrderId) : IQuery<List<StockMovementResponse>>;
