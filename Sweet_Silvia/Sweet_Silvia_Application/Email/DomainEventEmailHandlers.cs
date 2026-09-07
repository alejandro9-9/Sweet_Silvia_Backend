using MediatR;
using Microsoft.Extensions.Logging;
using Sweet_Silvia_Application.Abstractions.Email;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.User;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;
using UserEntity = Sweet_Silvia_Domain.User.User;

namespace Sweet_Silvia_Application.Email;

internal sealed class OrderCreatedEmailHandler(
    IRepository<OrderEntity> orders,
    IRepository<UserEntity> users,
    IEmailSender emailSender,
    ILogger<OrderCreatedEmailHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(notification.Id, cancellationToken);
        if (order is null)
        {
            return;
        }

        var user = await users.GetByIdAsync(order.UserId, cancellationToken);
        if (user is null)
        {
            return;
        }

        try
        {
            await emailSender.SendOrderConfirmationAsync(
                user.Email.Value,
                user.Name,
                order.Id.ToString("N"),
                order.Total.Amount,
                order.Total.Currency,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send order confirmation email for order {OrderId}", order.Id);
        }
    }
}

internal sealed class PaymentApprovedEmailHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<UserEntity> users,
    IEmailSender emailSender,
    ILogger<PaymentApprovedEmailHandler> logger)
    : INotificationHandler<PaymentApprovedEvent>
{
    public async Task Handle(PaymentApprovedEvent notification, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(notification.Id, cancellationToken);
        if (payment is null)
        {
            return;
        }

        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        var user = order is null ? null : await users.GetByIdAsync(order.UserId, cancellationToken);
        if (order is null || user is null)
        {
            return;
        }

        try
        {
            await emailSender.SendPaymentApprovedAsync(
                user.Email.Value,
                user.Name,
                order.Id.ToString("N"),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send payment approval email for payment {PaymentId}", payment.Id);
        }
    }
}

internal sealed class PaymentRejectedEmailHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<UserEntity> users,
    IEmailSender emailSender,
    ILogger<PaymentRejectedEmailHandler> logger)
    : INotificationHandler<PaymentRejectedEvent>
{
    public async Task Handle(PaymentRejectedEvent notification, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(notification.Id, cancellationToken);
        if (payment is null)
        {
            return;
        }

        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        var user = order is null ? null : await users.GetByIdAsync(order.UserId, cancellationToken);
        if (order is null || user is null)
        {
            return;
        }

        try
        {
            await emailSender.SendPaymentRejectedAsync(
                user.Email.Value,
                user.Name,
                order.Id.ToString("N"),
                payment.Observation,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send payment rejection email for payment {PaymentId}", payment.Id);
        }
    }
}
