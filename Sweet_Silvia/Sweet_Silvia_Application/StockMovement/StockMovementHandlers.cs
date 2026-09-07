using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.StockMovement;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using StockMovementEntity = Sweet_Silvia_Domain.StockMovement.StockMovement;

namespace Sweet_Silvia_Application.StockMovement;

internal sealed class CreateStockMovementCommandHandler(
    IRepository<ProductVariantEntity> variants,
    IRepository<StockMovementEntity> stockMovements,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateStockMovementCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        var variant = await variants.GetByIdAsync(request.ProductVariantId, cancellationToken);
        if (variant is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("ProductVariant", request.ProductVariantId));
        }

        if (variant.PhysicalStock != request.PreviousStock)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("Stock.Changed", "Current stock does not match the provided previous stock."));
        }

        var difference = request.NewStock - request.PreviousStock;
        if (difference == 0 || request.Quantity != Math.Abs(difference))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Stock.InvalidAdjustment",
                "Adjustment quantity must match the difference between previous and new stock."));
        }

        if (difference > 0)
        {
            variant.IncreaseStock(difference);
        }
        else if (difference < 0)
        {
            variant.DecreaseStock(Math.Abs(difference));
        }

        var movement = StockMovementEntity.Create(
            request.ProductVariantId,
            request.OrderId,
            request.Type,
            request.Quantity,
            request.PreviousStock,
            request.NewStock,
            request.Observation);

        stockMovements.Add(movement);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return movement.Id;
    }
}

internal sealed class GetStockMovementsQueryHandler(IRepository<StockMovementEntity> stockMovements)
    : IQueryHandler<GetStockMovementsQuery, List<StockMovementResponse>>
{
    public Task<Result<List<StockMovementResponse>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(stockMovements.Query().Select(ToResponse).ToList()));
    }

    internal static StockMovementResponse ToResponse(StockMovementEntity movement)
    {
        return new StockMovementResponse(
            movement.Id,
            movement.ProductVariantId,
            movement.OrderId,
            movement.Type,
            movement.Quantity,
            movement.PreviousStock,
            movement.NewStock,
            movement.CreatedAt,
            movement.Observation);
    }
}

internal sealed class GetStockMovementsByProductVariantQueryHandler(IRepository<StockMovementEntity> stockMovements)
    : IQueryHandler<GetStockMovementsByProductVariantQuery, List<StockMovementResponse>>
{
    public Task<Result<List<StockMovementResponse>>> Handle(GetStockMovementsByProductVariantQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(stockMovements.Query()
            .Where(movement => movement.ProductVariantId == request.ProductVariantId)
            .Select(GetStockMovementsQueryHandler.ToResponse)
            .ToList()));
    }
}

internal sealed class GetStockMovementsByOrderQueryHandler(IRepository<StockMovementEntity> stockMovements)
    : IQueryHandler<GetStockMovementsByOrderQuery, List<StockMovementResponse>>
{
    public Task<Result<List<StockMovementResponse>>> Handle(GetStockMovementsByOrderQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(stockMovements.Query()
            .Where(movement => movement.OrderId == request.OrderId)
            .Select(GetStockMovementsQueryHandler.ToResponse)
            .ToList()));
    }
}
