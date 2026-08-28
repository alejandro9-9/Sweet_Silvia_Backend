using Microsoft.Extensions.Logging;
using Sweet_Silvia_Application.Abstractions.Email;

namespace Sweet_Silvia_Infrastructure.Abstractions.Email;

internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailVerificationCodeAsync(string recipientEmail, string recipientName, string code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("LOCAL EMAIL to {RecipientEmail} ({RecipientName}): verification code {Code}", recipientEmail, recipientName, code);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(string recipientEmail, string recipientName, string code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("LOCAL EMAIL to {RecipientEmail} ({RecipientName}): password reset code {Code}", recipientEmail, recipientName, code);
        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationAsync(string recipientEmail, string recipientName, string orderNumber, decimal totalAmount, string currency, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("LOCAL EMAIL to {RecipientEmail} ({RecipientName}): order {OrderNumber} confirmed for {Currency} {TotalAmount}", recipientEmail, recipientName, orderNumber, currency, totalAmount);
        return Task.CompletedTask;
    }

    public Task SendPaymentApprovedAsync(string recipientEmail, string recipientName, string orderNumber, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("LOCAL EMAIL to {RecipientEmail} ({RecipientName}): payment approved for order {OrderNumber}", recipientEmail, recipientName, orderNumber);
        return Task.CompletedTask;
    }

    public Task SendPaymentRejectedAsync(string recipientEmail, string recipientName, string orderNumber, string? observation, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("LOCAL EMAIL to {RecipientEmail} ({RecipientName}): payment rejected for order {OrderNumber}. Observation: {Observation}", recipientEmail, recipientName, orderNumber, observation);
        return Task.CompletedTask;
    }

    public Task SendShipmentDispatchedAsync(string recipientEmail, string recipientName, string orderNumber, string courierName, string? trackingCode, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("LOCAL EMAIL to {RecipientEmail} ({RecipientName}): shipment dispatched for order {OrderNumber} by {CourierName}. Tracking: {TrackingCode}", recipientEmail, recipientName, orderNumber, courierName, trackingCode);
        return Task.CompletedTask;
    }
}
