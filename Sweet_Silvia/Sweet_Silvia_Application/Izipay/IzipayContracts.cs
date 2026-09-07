using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Payment;

namespace Sweet_Silvia_Application.Izipay;

public sealed record IzipayPaymentLinkContact(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Street,
    string PostalCode,
    string City,
    string State,
    string Country = "PE",
    string DocumentType = "DNI",
    string Document = "");

public sealed record IzipayPaymentLinkGatewayRequest(
    string TransactionId,
    string ProductDescription,
    decimal Amount,
    string Currency,
    DateTimeOffset ExpirationDate,
    string WayOfUse,
    string PayMethod,
    IzipayPaymentLinkContact Billing,
    IzipayPaymentLinkContact Shipping);

public sealed record IzipayPaymentLinkGatewayResponse(
    string TransactionId,
    string Status,
    string PaymentLinkId,
    string PaymentUrl,
    string Amount,
    string Currency,
    string State);

public sealed record IzipayPaymentLinkResponse(
    Guid PaymentId,
    string TransactionId,
    string PaymentLinkId,
    string PaymentUrl,
    string Amount,
    string Currency,
    string State);

public sealed record GenerateIzipayPaymentLinkCommand(
    Guid OrderId,
    Guid PaymentMethodId,
    string? ProductDescription,
    DateTimeOffset? ExpirationDate,
    string PayMethod,
    IzipayPaymentLinkContact Billing,
    IzipayPaymentLinkContact? Shipping) : ICommand<IzipayPaymentLinkResponse>;

public sealed record IzipayNotificationResponseData(IReadOnlyList<IzipayNotificationOrder> Order);

public sealed record IzipayNotificationOrder(
    string? CodeAuth,
    string? Currency,
    string? Amount,
    string? StateMessage,
    string? PaymentLinkId,
    string? OrderNumber,
    string? UniqueId,
    string? ReferenceNumber);

public sealed record ProcessIzipayNotificationCommand(
    string HeaderTransactionId,
    string? BodyTransactionId,
    string Code,
    string? Message,
    string? PayloadHttp,
    string? Signature,
    IzipayNotificationResponseData? Response) : ICommand<IzipayNotificationResult>;

public sealed record IzipayNotificationResult(Guid PaymentId, PaymentStatus Status);
