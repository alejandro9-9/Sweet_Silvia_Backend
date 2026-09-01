using System.Net.Http.Json;
using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Shipping;
using Sweet_Silvia_Application.Olva;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Infrastructure.Abstractions.Shipping;

internal sealed class OlvaCourierClient(HttpClient httpClient, OlvaOptions options) : IOlvaCourierClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<Result<OlvaValidationResponse>> ValidateAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<OlvaValidationResponse>("validate", cancellationToken);
    }

    public Task<Result<List<OlvaAgencyResponse>>> GetAgenciesAsync(
        string? q,
        string? department,
        string? province,
        string? district,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery([
            ("q", q),
            ("department", department),
            ("province", province),
            ("district", district)
        ]);

        return GetListAsync<OlvaAgencyResponse>($"agencies{query}", cancellationToken);
    }

    public Task<Result<List<string>>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<string>("locations/departments", cancellationToken);
    }

    public Task<Result<List<string>>> GetProvincesAsync(string department, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            return Task.FromResult(Result.Failure<List<string>>(new Error("Olva.DepartmentRequired", "Department is required.")));
        }

        return GetListAsync<string>($"locations/departments/{Uri.EscapeDataString(department.Trim())}/provinces", cancellationToken);
    }

    public Task<Result<List<string>>> GetDistrictsAsync(string department, string province, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            return Task.FromResult(Result.Failure<List<string>>(new Error("Olva.DepartmentRequired", "Department is required.")));
        }

        if (string.IsNullOrWhiteSpace(province))
        {
            return Task.FromResult(Result.Failure<List<string>>(new Error("Olva.ProvinceRequired", "Province is required.")));
        }

        return GetListAsync<string>(
            $"locations/departments/{Uri.EscapeDataString(department.Trim())}/provinces/{Uri.EscapeDataString(province.Trim())}/districts",
            cancellationToken);
    }

    public Task<Result<OlvaQuoteResponse>> QuoteAsync(OlvaQuoteRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
        {
            return Task.FromResult(Result.Failure<OlvaQuoteResponse>(new Error("Olva.InvalidQuote", "Origin and destination ubigeo are required.")));
        }

        return PostRawDataAsync("account/quote", new
        {
            origin = request.Origin.Trim(),
            destination = request.Destination.Trim(),
            delivery_type = request.DeliveryType,
            shipment_type = request.ShipmentType,
            weight = request.Weight,
            partner_rate = request.PartnerRate
        }, cancellationToken);
    }

    public Task<Result<OlvaTrackingResponse>> TrackAsync(string orderNumber, string? orderCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return Task.FromResult(Result.Failure<OlvaTrackingResponse>(new Error("Olva.OrderNumberRequired", "Order number is required.")));
        }

        return PostDataAsync<OlvaTrackingResponse>("track", new
        {
            orderNumber = orderNumber.Trim(),
            orderCode = string.IsNullOrWhiteSpace(orderCode) ? null : orderCode.Trim()
        }, cancellationToken);
    }

    private async Task<Result<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var configured = EnsureConfigured<T>();
        if (configured.IsFailure)
        {
            return configured;
        }

        using var response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<Result<List<T>>> GetListAsync<T>(string path, CancellationToken cancellationToken)
    {
        var configured = EnsureConfigured<List<T>>();
        if (configured.IsFailure)
        {
            return configured;
        }

        using var response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadListResponseAsync<T>(response, cancellationToken);
    }

    private async Task<Result<T>> PostDataAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        var configured = EnsureConfigured<T>();
        if (configured.IsFailure)
        {
            return configured;
        }

        using var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<Result<OlvaQuoteResponse>> PostRawDataAsync(string path, object body, CancellationToken cancellationToken)
    {
        var configured = EnsureConfigured<OlvaQuoteResponse>();
        if (configured.IsFailure)
        {
            return configured;
        }

        using var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        var documentResult = await ReadDocumentAsync(response, cancellationToken);
        if (documentResult.IsFailure)
        {
            return Result.Failure<OlvaQuoteResponse>(documentResult.Error);
        }

        var root = documentResult.Value.RootElement;
        var data = root.TryGetProperty("data", out var dataElement)
            ? dataElement.Clone()
            : root.Clone();

        return new OlvaQuoteResponse(data);
    }

    private static async Task<Result<T>> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var documentResult = await ReadDocumentAsync(response, cancellationToken);
        if (documentResult.IsFailure)
        {
            return Result.Failure<T>(documentResult.Error);
        }

        var root = documentResult.Value.RootElement;
        var payload = root.TryGetProperty("data", out var dataElement) ? dataElement : root;
        var value = payload.Deserialize<T>(JsonOptions);

        return value is null
            ? Result.Failure<T>(new Error("Olva.EmptyResponse", "Olva returned an empty response."))
            : Result.Success(value);
    }

    private static async Task<Result<List<T>>> ReadListResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var documentResult = await ReadDocumentAsync(response, cancellationToken);
        if (documentResult.IsFailure)
        {
            return Result.Failure<List<T>>(documentResult.Error);
        }

        var root = documentResult.Value.RootElement;
        var payload = root.TryGetProperty("data", out var dataElement) ? dataElement : root;
        var value = payload.Deserialize<List<T>>(JsonOptions);

        return value is null
            ? Result.Failure<List<T>>(new Error("Olva.EmptyResponse", "Olva returned an empty response."))
            : Result.Success(value);
    }

    private static async Task<Result<JsonDocument>> ReadDocumentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = TryGetErrorMessage(json) ?? $"Olva request failed with status {(int)response.StatusCode}.";
            return Result.Failure<JsonDocument>(new Error("Olva.RequestFailed", message));
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return Result.Failure<JsonDocument>(new Error("Olva.EmptyResponse", "Olva returned an empty response."));
        }

        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return Result.Failure<JsonDocument>(new Error("Olva.InvalidResponse", "Olva returned an invalid JSON response."));
        }
    }

    private Result<T> EnsureConfigured<T>()
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey) || options.ApiKey.Contains("PEGA_AQUI", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<T>(new Error("Olva.ApiKeyMissing", "Olva API key is not configured."));
        }

        return Result.Success(default(T)!);
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
            return document.RootElement.TryGetProperty("error", out var error)
                ? error.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildQuery(IEnumerable<(string Key, object? Value)> values)
    {
        var parameters = values
            .Where(value => value.Value is not null && !string.IsNullOrWhiteSpace(value.Value.ToString()))
            .Select(value => $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value!.ToString()!)}")
            .ToList();

        return parameters.Count == 0 ? string.Empty : $"?{string.Join("&", parameters)}";
    }
}
