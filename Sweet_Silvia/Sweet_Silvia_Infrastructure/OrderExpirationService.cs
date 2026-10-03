using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sweet_Silvia_Application.Abstractions.Orders;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.OrderItem;
using Sweet_Silvia_Domain.OrderStatusHistory;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.ProductVariant;
using Sweet_Silvia_Domain.StockMovement;
using Sweet_Silvia_Infrastructure.Abstractions.Orders;
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;

namespace Sweet_Silvia_Infrastructure;

internal sealed class OrderExpirationService(
    IServiceScopeFactory scopeFactory,
    OrderExpirationOptions options,
    ILogger<OrderExpirationService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(CleanupInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<Guid> expiredOrderIds;
            using (var scope = scopeFactory.CreateScope())
            {
                var orders = scope.ServiceProvider.GetRequiredService<IRepository<Order>>();
                var payments = scope.ServiceProvider.GetRequiredService<IRepository<PaymentEntity>>();
                var cutoff = DateTimeOffset.UtcNow.AddHours(-options.PendingPaymentExpirationHours);
                var now = DateTimeOffset.UtcNow;
                var orderIdsWithValidGatewaySessions = payments.Query()
                    .Where(payment => payment.Status == PaymentStatus.PendingGateway &&
                        (!payment.GatewayExpiresAt.HasValue || payment.GatewayExpiresAt.Value > now))
                    .Select(payment => payment.OrderId);
                expiredOrderIds = await orders.Query()
                    .Where(order => order.Status == OrderStatus.PendingReceipt && order.CreatedAt <= cutoff &&
                        !orderIdsWithValidGatewaySessions.Contains(order.Id))
                    .OrderBy(order => order.CreatedAt)
                    .Take(100)
                    .Select(order => order.Id)
                    .ToListAsync(cancellationToken);
            }

            foreach (var orderId in expiredOrderIds)
            {
                await ExpireOrderAsync(orderId, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The host is stopping.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not clean expired pending orders. It will retry on the next interval.");
        }
    }

    private async Task ExpireOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var orders = scope.ServiceProvider.GetRequiredService<IRepository<Order>>();
            var payments = scope.ServiceProvider.GetRequiredService<IRepository<PaymentEntity>>();
            var variants = scope.ServiceProvider.GetRequiredService<IRepository<ProductVariant>>();
            var stockMovements = scope.ServiceProvider.GetRequiredService<IRepository<StockMovement>>();
            var histories = scope.ServiceProvider.GetRequiredService<IRepository<OrderStatusHistory>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var order = await orders.GetByIdAsync(orderId, cancellationToken);
            if (order is null || order.Status != OrderStatus.PendingReceipt)
            {
                return;
            }

            var checkoutLock = scope.ServiceProvider.GetRequiredService<IUserCheckoutLock>();
            await using var userLock = await checkoutLock.AcquireAsync(order.UserId, cancellationToken);
            order = await orders.GetByIdAsync(orderId, cancellationToken);
            if (order is null || order.Status != OrderStatus.PendingReceipt)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            if (payments.Query().Any(payment =>
                    payment.OrderId == order.Id &&
                    payment.Status == PaymentStatus.PendingGateway &&
                    (!payment.GatewayExpiresAt.HasValue || payment.GatewayExpiresAt.Value > now)))
            {
                return;
            }

            var reservations = stockMovements.Query()
                .Where(movement => movement.OrderId == order.Id && movement.Type == StockMovementType.Reservation)
                .ToList();

            foreach (var reservation in reservations)
            {
                var variant = await variants.GetByIdAsync(reservation.ProductVariantId, cancellationToken)
                    ?? throw new InvalidOperationException($"Product variant '{reservation.ProductVariantId}' no longer exists.");

                var previousStock = variant.PhysicalStock;
                variant.IncreaseStock(reservation.Quantity);
                stockMovements.Add(StockMovement.Create(
                    variant.Id,
                    order.Id,
                    StockMovementType.Cancellation,
                    reservation.Quantity,
                    previousStock,
                    variant.PhysicalStock,
                    "Stock reservation released after the unpaid order expired."));
            }

            var previousStatus = order.Status;
            order.ChangeStatus(
                OrderStatus.Cancelled,
                observation: $"Automatically cancelled after {options.PendingPaymentExpirationHours} hours without payment.");
            histories.Add(OrderStatusHistory.Create(
                order.Id,
                previousStatus,
                OrderStatus.Cancelled,
                null,
                $"Automatically cancelled after {options.PendingPaymentExpirationHours} hours without payment."));

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Expired unpaid order {OrderId} and released its stock reservation.", order.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogDebug("Skipped expired order {OrderId} because it was updated concurrently.", orderId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The host is stopping.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not expire pending order {OrderId}.", orderId);
        }
    }
}
