using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Payments;
using Sweet_Silvia_Application.Izipay;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Infrastructure.Abstractions.Payments;

internal sealed class IzipayPaymentGateway(HttpClient httpClient, IzipayOptions options) : IIzipayPaymentGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<IzipayPaymentLinkGatewayResponse>> GeneratePaymentLinkAsync(
        IzipayPaymentLinkGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        var configured = EnsureConfigured();
        if (configured.IsFailure)
        {
            return Result.Failure<IzipayPaymentLinkGatewayResponse>(configured.Error);
        }

        var tokenResult = await GenerateTokenAsync(request.TransactionId, cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Failure<IzipayPaymentLinkGatewayResponse>(tokenResult.Error);
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, NormalizePath(options.PaymentLinkEndpoint))
        {
            Content = JsonContent.Create(BuildPaymentLinkBody(request), options: JsonOptions)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);
        httpRequest.Headers.TryAddWithoutValidation("transactionId", request.TransactionId);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var documentResult = await ReadDocumentAsync(response, cancellationToken);
        if (documentResult.IsFailure)
        {
            return Result.Failure<IzipayPaymentLinkGatewayResponse>(documentResult.Error);
        }

        var payload = GetPayload(documentResult.Value.RootElement);
        var paymentLinkId = GetString(payload, "paymentLinkId", "id", "linkId");
        var paymentUrl = GetString(payload, "urL_PaymentLink", "paymentUrl", "url", "link");

        if (string.IsNullOrWhiteSpace(paymentLinkId) || string.IsNullOrWhiteSpace(paymentUrl))
        {
            return Result.Failure<IzipayPaymentLinkGatewayResponse>(
                new Error("Izipay.InvalidResponse", "Izipay did not return a valid payment link."));
        }

        return new IzipayPaymentLinkGatewayResponse(
            request.TransactionId,
            GetString(payload, "status") ?? "OK",
            paymentLinkId,
            paymentUrl,
            GetString(payload, "amount") ?? request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            GetString(payload, "currency") ?? request.Currency,
            GetString(payload, "state") ?? string.Empty);
    }

    private async Task<Result<string>> GenerateTokenAsync(string transactionId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, NormalizePath(options.TokenEndpoint))
        {
            Content = JsonContent.Create(new
            {
                requestSource = "ECOMMERCE",
                merchantCode = options.MerchantCode.Trim(),
                orderNumber = transactionId,
                publicKey = options.PublicKey.Trim(),
                amount = "0.00"
            }, options: JsonOptions)
        };
        request.Headers.TryAddWithoutValidation("transactionId", transactionId);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var documentResult = await ReadDocumentAsync(response, cancellationToken);
        if (documentResult.IsFailure)
        {
            return Result.Failure<string>(documentResult.Error);
        }

        var payload = GetPayload(documentResult.Value.RootElement);
        var token = GetString(payload, "accessToken", "token", "bearerToken", "sessionToken");

        return string.IsNullOrWhiteSpace(token)
            ? Result.Failure<string>(new Error("Izipay.TokenMissing", "Izipay did not return an access token."))
            : Result.Success(token);
    }

    private object BuildPaymentLinkBody(IzipayPaymentLinkGatewayRequest request)
    {
        return new
        {
            merchantCode = options.MerchantCode.Trim(),
            transactionId = request.TransactionId,
            productDescription = request.ProductDescription,
            amount = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            currency = request.Currency,
            expirationDate = request.ExpirationDate.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            wayOfUse = request.WayOfUse,
            payMethod = request.PayMethod,
            email_Notification = request.Billing.Email.Trim(),
            referenceCode = request.TransactionId,
            languageUsed = "ES",
            urlIpn = options.NotificationUrl.Trim(),
            urL_Terms_and_Conditions = options.TermsAndConditionsUrl.Trim(),
            billing = ToContactBody(request.Billing),
            shipping = ToContactBody(request.Shipping)
        };
    }

    private static object ToContactBody(IzipayPaymentLinkContact contact)
    {
        return new
        {
            firstName = contact.FirstName.Trim(),
            lastName = contact.LastName.Trim(),
            email = contact.Email.Trim(),
            phoneNumber = contact.PhoneNumber.Trim(),
            street = contact.Street.Trim(),
            postalCode = contact.PostalCode.Trim(),
            city = contact.City.Trim(),
            state = contact.State.Trim(),
            country = contact.Country.Trim(),
            documentType = contact.DocumentType.Trim(),
            document = contact.Document.Trim()
        };
    }

    private Result EnsureConfigured()
    {
        if (!options.Enabled)
        {
            return Result.Failure(new Error("Izipay.Disabled", "Izipay integration is disabled."));
        }

        if (string.IsNullOrWhiteSpace(options.BaseUrl) ||
            string.IsNullOrWhiteSpace(options.TokenEndpoint) ||
            string.IsNullOrWhiteSpace(options.PaymentLinkEndpoint) ||
            string.IsNullOrWhiteSpace(options.MerchantCode) ||
            string.IsNullOrWhiteSpace(options.PublicKey) ||
            IsPlaceholder(options.MerchantCode) ||
            IsPlaceholder(options.PublicKey) ||
            string.IsNullOrWhiteSpace(options.HashKey) ||
            string.IsNullOrWhiteSpace(options.NotificationUrl) ||
            string.IsNullOrWhiteSpace(options.TermsAndConditionsUrl) ||
            !Uri.TryCreate(options.NotificationUrl, UriKind.Absolute, out _) ||
            !Uri.TryCreate(options.TermsAndConditionsUrl, UriKind.Absolute, out _) ||
            IsPlaceholder(options.HashKey) ||
            IsPlaceholder(options.NotificationUrl) ||
            IsPlaceholder(options.TermsAndConditionsUrl))
        {
            return Result.Failure(new Error("Izipay.ConfigurationMissing", "Izipay credentials are not configured."));
        }

        return Result.Success();
    }

    private static async Task<Result<JsonDocument>> ReadDocumentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = TryGetErrorMessage(json) ?? $"Izipay request failed with status {(int)response.StatusCode}.";
            return Result.Failure<JsonDocument>(new Error("Izipay.RequestFailed", message));
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return Result.Failure<JsonDocument>(new Error("Izipay.EmptyResponse", "Izipay returned an empty response."));
        }

        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return Result.Failure<JsonDocument>(new Error("Izipay.InvalidResponse", "Izipay returned an invalid JSON response."));
        }
    }

    private static JsonElement GetPayload(JsonElement root)
    {
        var payload = root;
        if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("response", out var response))
        {
            payload = response;
        }

        if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("data", out var data))
        {
            payload = data;
        }

        return payload;
    }

    private static string? TryGetErrorMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return GetString(root, "message", "error", "errorMessage", "description", "detail");
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var property))
            {
                continue;
            }

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        return null;
    }

    private static string NormalizePath(string path)
        => path.Trim().TrimStart('/');

    private static bool IsPlaceholder(string value)
        => value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("PEGA_AQUI", StringComparison.OrdinalIgnoreCase);
}
