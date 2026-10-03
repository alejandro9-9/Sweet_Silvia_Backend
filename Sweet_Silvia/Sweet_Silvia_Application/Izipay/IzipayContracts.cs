using System.Net.Mail;
using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Payment;

namespace Sweet_Silvia_Application.Izipay;

public sealed record IzipayCustomerDetails(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Street,
    string PostalCode,
    string City,
    string State,
    string Country = "PE");

public static class IzipayCustomerValidation
{
    public const string InvalidEmailMessage = "El correo de tu cuenta no es válido para Izipay. Actualízalo en Perfil con un correo real y vuelve a intentar.";

    public static string? Validate(IzipayCustomerDetails? customer)
    {
        if (customer is null)
        {
            return "Customer information is required to create an Izipay payment form.";
        }

        var email = customer.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254 ||
            !MailAddress.TryCreate(email, out var parsedEmail) ||
            !string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase) ||
            !parsedEmail.Host.Contains('.') ||
            new[] { ".local", ".localhost", ".test", ".example", ".invalid" }
                .Any(suffix => parsedEmail.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
        {
            return InvalidEmailMessage;
        }

        if (Exceeds(customer.FirstName, 100) || Exceeds(customer.LastName, 100) ||
            Exceeds(customer.PhoneNumber, 30) || Exceeds(customer.Street, 200) ||
            Exceeds(customer.PostalCode, 30) || Exceeds(customer.City, 100) ||
            Exceeds(customer.State, 100) || customer.Country?.Trim().Length != 2)
        {
            return "Customer billing details exceed Izipay's supported field lengths.";
        }

        return null;
    }

    private static bool Exceeds(string? value, int maximumLength)
        => value?.Trim().Length > maximumLength;
}

public sealed record IzipayPaymentFormGatewayRequest(
    string OrderReference,
    decimal Amount,
    string Currency,
    IzipayCustomerDetails Customer);

public sealed record IzipayPaymentFormGatewayResponse(string FormToken, string PublicKey);

public sealed record IzipayPaymentFormResponse(
    Guid PaymentId,
    Guid OrderId,
    string OrderReference,
    string FormToken,
    string PublicKey,
    long Amount,
    string Currency);

public sealed record GenerateIzipayPaymentFormCommand(
    Guid OrderId,
    Guid PaymentMethodId,
    IzipayCustomerDetails Customer,
    bool ReplaceActivePayment = false) : ICommand<IzipayPaymentFormResponse>;

public sealed record ProcessIzipayNotificationCommand(
    string Answer,
    string Signature,
    string HashAlgorithm,
    string ExpectedShopId,
    Guid? ExpectedPaymentId = null,
    bool IsServerNotification = false) : ICommand<IzipayNotificationResult>;

public sealed record IzipayNotificationResult(Guid PaymentId, PaymentStatus Status);
