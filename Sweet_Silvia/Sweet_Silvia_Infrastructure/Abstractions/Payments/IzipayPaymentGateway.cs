using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Payments;
using Sweet_Silvia_Application.Izipay;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Infrastructure.Abstractions.Payments;

internal sealed class IzipayPaymentGateway(HttpClient httpClient, IzipayOptions options) : IIzipayPaymentGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<IzipayPaymentFormGatewayResponse>> CreatePaymentFormAsync(
        IzipayPaymentFormGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.ShopId) || string.IsNullOrWhiteSpace(options.Password))
        {
            return Result.Failure<IzipayPaymentFormGatewayResponse>(
                new Error("Izipay.ConfigurationMissing", "Izipay REST credentials are not configured."));
        }

        if (!long.TryParse(options.ShopId, NumberStyles.None, CultureInfo.InvariantCulture, out _) ||
            string.IsNullOrWhiteSpace(options.PublicKey))
        {
            return Result.Failure<IzipayPaymentFormGatewayResponse>(
                new Error("Izipay.ConfigurationMissing", "Izipay shop id and public key are required."));
        }

        var amountMinorUnits = decimal.Round(request.Amount * 100m, 0, MidpointRounding.AwayFromZero);
        if (request.Amount <= 0 || amountMinorUnits > long.MaxValue || amountMinorUnits / 100m != request.Amount)
        {
            return Result.Failure<IzipayPaymentFormGatewayResponse>(
                new Error("Izipay.InvalidAmount", "Izipay amount must be positive and have no more than two decimal places."));
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.PaymentEndpoint.TrimStart('/'))
        {
            Content = JsonContent.Create(new
            {
                amount = (long)amountMinorUnits,
                currency = request.Currency.ToUpperInvariant(),
                orderId = request.OrderReference,
                formAction = "PAYMENT",
                ipnTargetUrl = options.NotificationUrl,
                customer = new
                {
                    email = request.Customer.Email?.Trim() ?? string.Empty,
                    billingDetails = new
                    {
                        firstName = request.Customer.FirstName?.Trim() ?? string.Empty,
                        lastName = request.Customer.LastName?.Trim() ?? string.Empty,
                        phoneNumber = request.Customer.PhoneNumber?.Trim() ?? string.Empty,
                        address = request.Customer.Street?.Trim() ?? string.Empty,
                        city = request.Customer.City?.Trim() ?? string.Empty,
                        state = request.Customer.State?.Trim() ?? string.Empty,
                        country = request.Customer.Country?.Trim().ToUpperInvariant() ?? string.Empty
                    }
                }
            }, options: JsonOptions)
        };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.ShopId.Trim()}:{options.Password}"));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<IzipayPaymentFormGatewayResponse>(new Error(
                "Izipay.RequestFailed",
                TryGetErrorMessage(content) ?? $"Izipay request failed with status {(int)response.StatusCode}."));
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var answer = GetProperty(root, "answer");
            var status = GetString(root, "status") ?? GetString(answer, "status");
            var formToken = GetString(answer, "formToken") ?? GetString(root, "formToken");

            if (!string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(formToken))
            {
                var gatewayMessage = GetString(root, "serverErrorMessage", "errorMessage", "message", "detailedErrorMessage") ??
                                     GetString(answer, "serverErrorMessage", "errorMessage", "message", "detailedErrorMessage");
                if (gatewayMessage?.Contains("invalid customer email", StringComparison.OrdinalIgnoreCase) == true)
                {
                    gatewayMessage = IzipayCustomerValidation.InvalidEmailMessage;
                }

                return Result.Failure<IzipayPaymentFormGatewayResponse>(new Error(
                    "Izipay.InvalidResponse",
                    gatewayMessage ?? $"Izipay returned status '{status ?? "missing"}' without a valid payment form token."));
            }

            var publicKey = options.PublicKey.Trim();
            var shopPrefix = $"{options.ShopId.Trim()}:";
            if (!publicKey.StartsWith(shopPrefix, StringComparison.Ordinal))
            {
                publicKey = shopPrefix + publicKey;
            }

            return new IzipayPaymentFormGatewayResponse(formToken, publicKey);
        }
        catch (JsonException)
        {
            return Result.Failure<IzipayPaymentFormGatewayResponse>(
                new Error("Izipay.InvalidResponse", "Izipay returned an invalid JSON response."));
        }
    }

    private static JsonElement GetProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return default;
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var property = GetProperty(element, name);
            if (property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static string? TryGetErrorMessage(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var answer = GetProperty(root, "answer");
            return GetString(root, "serverErrorMessage", "errorMessage", "message", "description", "detailedErrorMessage") ??
                   GetString(answer, "serverErrorMessage", "errorMessage", "message", "description", "detailedErrorMessage");
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
