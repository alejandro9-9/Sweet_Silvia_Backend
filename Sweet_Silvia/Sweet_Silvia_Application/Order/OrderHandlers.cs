using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.OrderItem;
using Sweet_Silvia_Application.OrderStatusHistory;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using OrderStatusHistoryEntity = Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory;

namespace Sweet_Silvia_Application.Order;

internal sealed class CreateOrderCommandHandler(IRepository<OrderEntity> orders, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = OrderEntity.Create(
            request.UserId,
            request.AddressId,
            request.CouponId,
            Money.Create(request.Subtotal, request.Currency),
            Money.Create(request.DiscountTotal, request.Currency),
            Money.Create(request.ShippingCost, request.Currency),
            Money.Create(request.Total, request.Currency));

        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}

internal sealed class AddOrderItemCommandHandler(
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddOrderItemCommand>
{
    public async Task<Result> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        var item = OrderItemEntity.Create(
            request.OrderId,
            request.ProductVariantId,
            request.ProductName,
            request.Sku,
            request.Size,
            request.Color,
            request.Quantity,
            Money.Create(request.UnitPrice, request.Currency),
            Money.Create(request.UnitDiscount, request.Currency),
            Money.Create(request.Subtotal, request.Currency));

        orderItems.Add(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ChangeOrderStatusCommandHandler(
    IRepository<OrderEntity> orders,
    IRepository<OrderStatusHistoryEntity> histories,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeOrderStatusCommand>
{
    public async Task<Result> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        var previousStatus = order.Status;
        order.ChangeStatus(request.Status, request.ChangedByUserId, request.Observation);
        histories.Add(OrderStatusHistoryEntity.Create(request.OrderId, previousStatus, request.Status, request.ChangedByUserId, request.Observation));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetOrderByIdQueryHandler(IRepository<OrderEntity> orders)
    : IQueryHandler<GetOrderByIdQuery, OrderResponse>
{
    public Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = orders.Query().FirstOrDefault(order => order.Id == request.Id);
        return Task.FromResult(order is null
            ? Result.Failure<OrderResponse>(ApplicationErrors.NotFound("Order", request.Id))
            : Result.Success(OrderMappings.ToResponse(order)));
    }
}

internal sealed class GetOrdersQueryHandler(IRepository<OrderEntity> orders)
    : IQueryHandler<GetOrdersQuery, List<OrderResponse>>
{
    public Task<Result<List<OrderResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orders.Query();
        if (request.Status.HasValue)
        {
            query = query.Where(order => order.Status == request.Status.Value);
        }

        return Task.FromResult(Result.Success(query.Select(OrderMappings.ToResponse).ToList()));
    }
}

internal sealed class GetOrdersByUserQueryHandler(IRepository<OrderEntity> orders)
    : IQueryHandler<GetOrdersByUserQuery, List<OrderResponse>>
{
    public Task<Result<List<OrderResponse>>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(orders.Query()
            .Where(order => order.UserId == request.UserId)
            .Select(OrderMappings.ToResponse)
            .ToList()));
    }
}

internal sealed class GetOrderItemsByOrderQueryHandler(IRepository<OrderItemEntity> orderItems)
    : IQueryHandler<GetOrderItemsByOrderQuery, List<OrderItemResponse>>
{
    public Task<Result<List<OrderItemResponse>>> Handle(GetOrderItemsByOrderQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(orderItems.Query()
            .Where(item => item.OrderId == request.OrderId)
            .Select(OrderMappings.ToItemResponse)
            .ToList()));
    }
}

internal sealed class GetOrderStatusHistoryQueryHandler(IRepository<OrderStatusHistoryEntity> histories)
    : IQueryHandler<GetOrderStatusHistoryQuery, List<OrderStatusHistoryResponse>>
{
    public Task<Result<List<OrderStatusHistoryResponse>>> Handle(GetOrderStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(histories.Query()
            .Where(history => history.OrderId == request.OrderId)
            .OrderBy(history => history.ChangedAt)
            .Select(OrderMappings.ToHistoryResponse)
            .ToList()));
    }
}

internal static class OrderMappings
{
    public static OrderResponse ToResponse(OrderEntity order)
    {
        return new OrderResponse(
            order.Id,
            order.UserId,
            order.AddressId,
            order.CouponId,
            order.Status,
            order.Subtotal.Amount,
            order.DiscountTotal.Amount,
            order.ShippingCost.Amount,
            order.Total.Amount,
            order.Total.Currency,
            order.CreatedAt,
            order.UpdatedAt);
    }

    public static OrderItemResponse ToItemResponse(OrderItemEntity item)
    {
        return new OrderItemResponse(
            item.Id,
            item.OrderId,
            item.ProductVariantId,
            item.HistoricalProductName,
            item.HistoricalSku,
            item.HistoricalSize,
            item.HistoricalColor,
            item.Quantity,
            item.HistoricalUnitPrice.Amount,
            item.HistoricalUnitDiscount.Amount,
            item.Subtotal.Amount,
            item.Subtotal.Currency);
    }

    public static OrderStatusHistoryResponse ToHistoryResponse(OrderStatusHistoryEntity history)
    {
        return new OrderStatusHistoryResponse(
            history.Id,
            history.OrderId,
            history.PreviousStatus,
            history.NewStatus,
            history.ChangedByUserId,
            history.ChangedAt,
            history.Observation);
    }
}
