using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Sweet_Silvia_Application.Abstractions.Email;

namespace Sweet_Silvia_Infrastructure.Abstractions.Email;

internal sealed class BrevoEmailSender(HttpClient httpClient, EmailDeliveryOptions deliveryOptions, EmailVerificationOptions verificationOptions)
    : IEmailSender
{
    public Task SendEmailVerificationCodeAsync(string recipientEmail, string recipientName, string code, CancellationToken cancellationToken = default)
    {
        var html = $"<p>Hola {Encode(recipientName)},</p><p>Tu codigo de verificacion de Sweet Silvia es:</p><p style=\"font-size:28px;font-weight:700;letter-spacing:6px\">{Encode(code)}</p><p>Este codigo vencera en {verificationOptions.CodeExpirationMinutes} minutos.</p>";
        var text = $"Hola {recipientName}. Tu codigo de verificacion de Sweet Silvia es {code}. Vencera en {verificationOptions.CodeExpirationMinutes} minutos.";
        return SendAsync(recipientEmail, recipientName, "Verifica tu correo en Sweet Silvia", html, text, "verify-email", cancellationToken);
    }

    public Task SendPasswordResetCodeAsync(string recipientEmail, string recipientName, string code, CancellationToken cancellationToken = default)
    {
        var html = $"<p>Hola {Encode(recipientName)},</p><p>Tu codigo para restablecer la contrasena es:</p><p style=\"font-size:28px;font-weight:700;letter-spacing:6px\">{Encode(code)}</p><p>Este codigo vencera en {verificationOptions.CodeExpirationMinutes} minutos. Si no solicitaste el cambio, ignora este correo.</p>";
        var text = $"Hola {recipientName}. Tu codigo para restablecer la contrasena de Sweet Silvia es {code}. Vencera en {verificationOptions.CodeExpirationMinutes} minutos.";
        return SendAsync(recipientEmail, recipientName, "Recupera tu contrasena de Sweet Silvia", html, text, "reset-password", cancellationToken);
    }

    public Task SendOrderConfirmationAsync(string recipientEmail, string recipientName, string orderNumber, decimal totalAmount, string currency, CancellationToken cancellationToken = default)
    {
        var html = $"<p>Hola {Encode(recipientName)},</p><p>Recibimos tu pedido <strong>{Encode(orderNumber)}</strong>.</p><p>Total: <strong>{Encode(currency)} {totalAmount:N2}</strong>.</p>";
        var text = $"Hola {recipientName}. Recibimos tu pedido {orderNumber}. Total: {currency} {totalAmount:N2}.";
        return SendAsync(recipientEmail, recipientName, $"Confirmacion de pedido {orderNumber}", html, text, "order-confirmation", cancellationToken);
    }

    public Task SendPaymentApprovedAsync(string recipientEmail, string recipientName, string orderNumber, CancellationToken cancellationToken = default)
    {
        var html = $"<p>Hola {Encode(recipientName)},</p><p>Tu pago del pedido <strong>{Encode(orderNumber)}</strong> fue aprobado. Empezaremos a preparar tu compra.</p>";
        var text = $"Hola {recipientName}. Tu pago del pedido {orderNumber} fue aprobado. Empezaremos a preparar tu compra.";
        return SendAsync(recipientEmail, recipientName, $"Pago aprobado - Pedido {orderNumber}", html, text, "payment-approved", cancellationToken);
    }

    public Task SendPaymentRejectedAsync(string recipientEmail, string recipientName, string orderNumber, string? observation, CancellationToken cancellationToken = default)
    {
        var detail = string.IsNullOrWhiteSpace(observation) ? string.Empty : $"<p>Observacion: {Encode(observation)}</p>";
        var html = $"<p>Hola {Encode(recipientName)},</p><p>No pudimos aprobar el pago del pedido <strong>{Encode(orderNumber)}</strong>.</p>{detail}";
        var text = $"Hola {recipientName}. No pudimos aprobar el pago del pedido {orderNumber}. {observation}".Trim();
        return SendAsync(recipientEmail, recipientName, $"Pago observado - Pedido {orderNumber}", html, text, "payment-rejected", cancellationToken);
    }

    public Task SendShipmentDispatchedAsync(string recipientEmail, string recipientName, string orderNumber, string courierName, string? trackingCode, CancellationToken cancellationToken = default)
    {
        var tracking = string.IsNullOrWhiteSpace(trackingCode) ? string.Empty : $"<p>Codigo de seguimiento: <strong>{Encode(trackingCode)}</strong></p>";
        var html = $"<p>Hola {Encode(recipientName)},</p><p>Tu pedido <strong>{Encode(orderNumber)}</strong> fue despachado por {Encode(courierName)}.</p>{tracking}";
        var text = $"Hola {recipientName}. Tu pedido {orderNumber} fue despachado por {courierName}. Codigo de seguimiento: {trackingCode}".Trim();
        return SendAsync(recipientEmail, recipientName, $"Pedido despachado {orderNumber}", html, text, "shipment-dispatched", cancellationToken);
    }

    private async Task SendAsync(string recipientEmail, string recipientName, string subject, string htmlContent, string textContent, string idempotencyPrefix, CancellationToken cancellationToken)
    {
        var sender = new MailAddress(deliveryOptions.From);
        var senderName = string.IsNullOrWhiteSpace(sender.DisplayName) ? "Sweet Silvia" : sender.DisplayName;

        using var request = new HttpRequestMessage(HttpMethod.Post, "smtp/email");
        request.Headers.Add("api-key", deliveryOptions.ApiKey);
        request.Headers.Add("Idempotency-Key", $"{idempotencyPrefix}-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(new
        {
            sender = new
            {
                name = senderName,
                email = sender.Address
            },
            to = new[]
            {
                new
                {
                    email = recipientEmail,
                    name = recipientName
                }
            },
            subject,
            htmlContent,
            textContent
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Brevo rejected the email ({(int)response.StatusCode}): {responseBody}",
            null,
            response.StatusCode);
    }

    private static string Encode(string value)
    {
        return HtmlEncoder.Default.Encode(value);
    }
}
