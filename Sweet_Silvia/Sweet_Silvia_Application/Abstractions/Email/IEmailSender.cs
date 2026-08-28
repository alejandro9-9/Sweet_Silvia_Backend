namespace Sweet_Silvia_Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendEmailVerificationCodeAsync(
        string recipientEmail,
        string recipientName,
        string code,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetCodeAsync(
        string recipientEmail,
        string recipientName,
        string code,
        CancellationToken cancellationToken = default);

    Task SendOrderConfirmationAsync(
        string recipientEmail,
        string recipientName,
        string orderNumber,
        decimal totalAmount,
        string currency,
        CancellationToken cancellationToken = default);

    Task SendPaymentApprovedAsync(
        string recipientEmail,
        string recipientName,
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task SendPaymentRejectedAsync(
        string recipientEmail,
        string recipientName,
        string orderNumber,
        string? observation,
        CancellationToken cancellationToken = default);

    Task SendShipmentDispatchedAsync(
        string recipientEmail,
        string recipientName,
        string orderNumber,
        string courierName,
        string? trackingCode,
        CancellationToken cancellationToken = default);
}
