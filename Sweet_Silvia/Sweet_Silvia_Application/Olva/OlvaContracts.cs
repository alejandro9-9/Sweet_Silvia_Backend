using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Olva;

public sealed record OlvaValidationResponse(
    bool Valid,
    string? UserId,
    int? Limit,
    int? CurrentUsage,
    int? Remaining);

public sealed record OlvaAgencyResponse(
    string? Code,
    string? Name,
    string? Type,
    string? Department,
    string? Province,
    string? District,
    string? Address,
    string? Phone,
    JsonElement? Schedule,
    double? Latitude,
    double? Longitude);

public sealed record CachedOlvaAgencyResponse(
    Guid Id,
    string Code,
    string Name,
    string? Type,
    string? Department,
    string? Province,
    string? District,
    string? Address,
    string? Phone,
    string? ScheduleJson,
    double? Latitude,
    double? Longitude,
    DateTimeOffset LastSyncedAt);

public sealed record OlvaQuoteLocationResponse(
    string Ubigeo,
    string Department,
    string Province,
    string District,
    int AgencyCount);

public sealed record SyncOlvaAgenciesResponse(int Imported, int Updated, int Deactivated);

public sealed record OlvaQuoteRequest(
    string Origin,
    string Destination,
    string? DeliveryType,
    int? ShipmentType,
    decimal? Weight,
    bool? PartnerRate,
    decimal? Length,
    decimal? Width,
    decimal? Height);

public sealed record OlvaQuoteResponse(JsonElement Data);

public sealed record OlvaCheckoutQuoteResponse(
    decimal Cost,
    string Currency,
    decimal ActualWeightKg,
    decimal VolumetricWeightKg,
    decimal ChargeableWeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    bool IsLocalEstimate = false);

public sealed record OlvaTrackingLocationResponse(string? Agency, string? Department);

public sealed record OlvaTrackingEventResponse(
    DateOnly? Date,
    string? Status,
    string? Detail,
    string? Location);

public sealed record OlvaTrackingResponse(
    string? TrackingNumber,
    string? Status,
    string? StatusDetail,
    OlvaTrackingLocationResponse? Origin,
    OlvaTrackingLocationResponse? Destination,
    DateOnly? EstimatedDelivery,
    DateTimeOffset? DeliveredAt,
    List<OlvaTrackingEventResponse> Events);

public sealed record ValidateOlvaApiKeyQuery : IQuery<OlvaValidationResponse>;

public sealed record GetOlvaAgenciesQuery(
    string? Q,
    string? Department,
    string? Province,
    string? District,
    int Page = 1,
    int PageSize = 50) : IQuery<List<CachedOlvaAgencyResponse>>;

public sealed record GetCachedOlvaDepartmentsQuery : IQuery<List<string>>;

public sealed record GetCachedOlvaProvincesQuery(string Department) : IQuery<List<string>>;

public sealed record GetCachedOlvaDistrictsQuery(string Department, string Province) : IQuery<List<string>>;

public sealed record GetOlvaQuoteLocationsQuery : IQuery<List<OlvaQuoteLocationResponse>>;

public sealed record SyncOlvaAgenciesCommand(
    string? Q,
    string? Department,
    string? Province,
    string? District) : ICommand<SyncOlvaAgenciesResponse>;

public sealed record GetOlvaDepartmentsQuery : IQuery<List<string>>;

public sealed record GetOlvaProvincesQuery(string Department) : IQuery<List<string>>;

public sealed record GetOlvaDistrictsQuery(string Department, string Province) : IQuery<List<string>>;

public sealed record QuoteOlvaShipmentQuery(
    string Origin,
    string Destination,
    string? DeliveryType,
    int? ShipmentType,
    decimal? Weight,
    bool? PartnerRate,
    decimal? Length,
    decimal? Width,
    decimal? Height) : IQuery<OlvaQuoteResponse>;

public sealed record QuoteCustomerOlvaShipmentQuery(Guid UserId, Guid DistrictId, Guid AgencyId)
    : IQuery<OlvaCheckoutQuoteResponse>;

public sealed record TrackOlvaShipmentQuery(string OrderNumber, string? OrderCode) : IQuery<OlvaTrackingResponse>;
