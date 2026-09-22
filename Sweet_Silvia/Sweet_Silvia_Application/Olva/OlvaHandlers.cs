using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Shipping;
using Sweet_Silvia_Domain.Abstractions;
using OlvaAgencyEntity = Sweet_Silvia_Domain.OlvaAgency.OlvaAgency;
using DepartmentEntity = Sweet_Silvia_Domain.Department.Department;
using DistrictEntity = Sweet_Silvia_Domain.District.District;
using ProvinceEntity = Sweet_Silvia_Domain.Province.Province;

namespace Sweet_Silvia_Application.Olva;

internal sealed class ValidateOlvaApiKeyQueryHandler(IOlvaCourierClient olvaCourierClient)
    : IQueryHandler<ValidateOlvaApiKeyQuery, OlvaValidationResponse>
{
    public Task<Result<OlvaValidationResponse>> Handle(ValidateOlvaApiKeyQuery request, CancellationToken cancellationToken)
    {
        return olvaCourierClient.ValidateAsync(cancellationToken);
    }
}

internal sealed class GetOlvaAgenciesQueryHandler(IRepository<OlvaAgencyEntity> olvaAgencies)
    : IQueryHandler<GetOlvaAgenciesQuery, List<CachedOlvaAgencyResponse>>
{
    public Task<Result<List<CachedOlvaAgencyResponse>>> Handle(GetOlvaAgenciesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var query = olvaAgencies.Query().Where(agency => agency.IsActive).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var search = request.Q.Trim().ToLower();
            query = query.Where(agency =>
                agency.Code.ToLower().Contains(search) ||
                agency.Name.ToLower().Contains(search) ||
                (agency.Address != null && agency.Address.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(agency => OlvaLocationNames.Matches(agency.Department, request.Department));
        }

        if (!string.IsNullOrWhiteSpace(request.Province))
        {
            query = query.Where(agency => OlvaLocationNames.Matches(agency.Province, request.Province));
        }

        if (!string.IsNullOrWhiteSpace(request.District))
        {
            query = query.Where(agency => OlvaLocationNames.Matches(agency.District, request.District));
        }

        var agencies = query
            .OrderBy(agency => agency.Department)
            .ThenBy(agency => agency.Province)
            .ThenBy(agency => agency.District)
            .ThenBy(agency => agency.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(OlvaAgencyMappings.ToCachedAgencyResponse)
            .ToList();

        return Task.FromResult(Result.Success(agencies));
    }
}

internal sealed class SyncOlvaAgenciesCommandHandler(
    IOlvaCourierClient olvaCourierClient,
    IRepository<OlvaAgencyEntity> olvaAgencies,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SyncOlvaAgenciesCommand, SyncOlvaAgenciesResponse>
{
    public async Task<Result<SyncOlvaAgenciesResponse>> Handle(SyncOlvaAgenciesCommand request, CancellationToken cancellationToken)
    {
        var result = await olvaCourierClient.GetAgenciesAsync(request.Q, request.Department, request.Province, request.District, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<SyncOlvaAgenciesResponse>(result.Error);
        }

        var existingAgencies = olvaAgencies.Query().ToDictionary(agency => agency.Code, StringComparer.OrdinalIgnoreCase);
        var receivedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var imported = 0;
        var updated = 0;

        foreach (var agency in result.Value)
        {
            var code = agency.Code?.Trim();
            var name = agency.Name?.Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            receivedCodes.Add(code);
            var scheduleJson = agency.Schedule?.GetRawText();

            if (existingAgencies.TryGetValue(code, out var existingAgency))
            {
                existingAgency.UpdateFromOlva(
                    name,
                    agency.Type,
                    agency.Department,
                    agency.Province,
                    agency.District,
                    agency.Address,
                    agency.Phone,
                    scheduleJson,
                    agency.Latitude,
                    agency.Longitude);
                updated++;
                continue;
            }

            olvaAgencies.Add(OlvaAgencyEntity.Create(
                code,
                name,
                agency.Type,
                agency.Department,
                agency.Province,
                agency.District,
                agency.Address,
                agency.Phone,
                scheduleJson,
                agency.Latitude,
                agency.Longitude));
            imported++;
        }

        var deactivated = 0;
        if (string.IsNullOrWhiteSpace(request.Q))
        {
            var scopedExisting = existingAgencies.Values.Where(agency =>
                OlvaLocationNames.Matches(agency.Department, request.Department) &&
                OlvaLocationNames.Matches(agency.Province, request.Province) &&
                OlvaLocationNames.Matches(agency.District, request.District));

            foreach (var agency in scopedExisting)
            {
                if (!receivedCodes.Contains(agency.Code))
                {
                    agency.Deactivate();
                    deactivated++;
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SyncOlvaAgenciesResponse(imported, updated, deactivated));
    }

}

internal sealed class GetCachedOlvaDepartmentsQueryHandler(IRepository<OlvaAgencyEntity> olvaAgencies)
    : IQueryHandler<GetCachedOlvaDepartmentsQuery, List<string>>
{
    public Task<Result<List<string>>> Handle(GetCachedOlvaDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = olvaAgencies.Query()
            .Where(agency => agency.IsActive && agency.Department != null)
            .AsEnumerable()
            .Select(agency => agency.Department!)
            .Where(OlvaLocationNames.IsValid)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        return Task.FromResult(Result.Success(departments));
    }
}

internal sealed class GetCachedOlvaProvincesQueryHandler(IRepository<OlvaAgencyEntity> olvaAgencies)
    : IQueryHandler<GetCachedOlvaProvincesQuery, List<string>>
{
    public Task<Result<List<string>>> Handle(GetCachedOlvaProvincesQuery request, CancellationToken cancellationToken)
    {
        var department = OlvaLocationNames.Normalize(request.Department);
        var provinces = olvaAgencies.Query()
            .Where(agency => agency.IsActive && agency.Department != null && agency.Province != null)
            .AsEnumerable()
            .Where(agency => OlvaLocationNames.Normalize(agency.Department) == department)
            .Select(agency => agency.Province!)
            .Where(OlvaLocationNames.IsValid)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        return Task.FromResult(Result.Success(provinces));
    }
}

internal sealed class GetCachedOlvaDistrictsQueryHandler(IRepository<OlvaAgencyEntity> olvaAgencies)
    : IQueryHandler<GetCachedOlvaDistrictsQuery, List<string>>
{
    public Task<Result<List<string>>> Handle(GetCachedOlvaDistrictsQuery request, CancellationToken cancellationToken)
    {
        var department = OlvaLocationNames.Normalize(request.Department);
        var province = OlvaLocationNames.Normalize(request.Province);
        var districts = olvaAgencies.Query()
            .Where(agency => agency.IsActive && agency.Department != null && agency.Province != null && agency.District != null)
            .AsEnumerable()
            .Where(agency => OlvaLocationNames.Normalize(agency.Department) == department && OlvaLocationNames.Normalize(agency.Province) == province)
            .Select(agency => agency.District!)
            .Where(OlvaLocationNames.IsValid)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        return Task.FromResult(Result.Success(districts));
    }
}

internal sealed class GetOlvaQuoteLocationsQueryHandler(
    IRepository<OlvaAgencyEntity> olvaAgencies,
    IRepository<DepartmentEntity> departments,
    IRepository<ProvinceEntity> provinces,
    IRepository<DistrictEntity> districts)
    : IQueryHandler<GetOlvaQuoteLocationsQuery, List<OlvaQuoteLocationResponse>>
{
    public Task<Result<List<OlvaQuoteLocationResponse>>> Handle(GetOlvaQuoteLocationsQuery request, CancellationToken cancellationToken)
    {
        var localLocations = (
            from district in districts.Query()
            join province in provinces.Query() on district.ProvinceId equals province.Id
            join department in departments.Query() on province.DepartmentId equals department.Id
            where district.IsActive && province.IsActive && department.IsActive && district.Ubigeo != null
            select new
            {
                Department = department.Name,
                Province = province.Name,
                District = district.Name,
                district.Ubigeo
            }).ToList();

        var locations = olvaAgencies.Query()
            .Where(agency => agency.IsActive)
            .AsEnumerable()
            .Select(agency =>
            {
                var location = localLocations.FirstOrDefault(candidate =>
                    OlvaLocationNames.Matches(candidate.Department, agency.Department) &&
                    OlvaLocationNames.Matches(candidate.Province, agency.Province) &&
                    OlvaLocationNames.Matches(candidate.District, agency.District) &&
                    IsUbigeo(candidate.Ubigeo));

                return location is null
                    ? null
                    : new
                    {
                        location.Ubigeo,
                        location.Department,
                        location.Province,
                        location.District,
                        AgencyCode = agency.Code
                    };
            })
            .Where(location => location is not null)
            .GroupBy(location => location!.Ubigeo!, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First()!;
                return new OlvaQuoteLocationResponse(
                    group.Key,
                    first.Department,
                    first.Province,
                    first.District,
                    group.Select(location => location!.AgencyCode).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            })
            .OrderBy(location => location.Department)
            .ThenBy(location => location.Province)
            .ThenBy(location => location.District)
            .ToList();

        return Task.FromResult(Result.Success(locations));
    }

    private static bool IsUbigeo(string? value)
        => value is { Length: 6 } && value.All(char.IsDigit);
}

internal sealed class GetOlvaDepartmentsQueryHandler(IOlvaCourierClient olvaCourierClient)
    : IQueryHandler<GetOlvaDepartmentsQuery, List<string>>
{
    public Task<Result<List<string>>> Handle(GetOlvaDepartmentsQuery request, CancellationToken cancellationToken)
    {
        return olvaCourierClient.GetDepartmentsAsync(cancellationToken);
    }
}

internal sealed class GetOlvaProvincesQueryHandler(IOlvaCourierClient olvaCourierClient)
    : IQueryHandler<GetOlvaProvincesQuery, List<string>>
{
    public Task<Result<List<string>>> Handle(GetOlvaProvincesQuery request, CancellationToken cancellationToken)
    {
        return olvaCourierClient.GetProvincesAsync(request.Department, cancellationToken);
    }
}

internal sealed class GetOlvaDistrictsQueryHandler(IOlvaCourierClient olvaCourierClient)
    : IQueryHandler<GetOlvaDistrictsQuery, List<string>>
{
    public Task<Result<List<string>>> Handle(GetOlvaDistrictsQuery request, CancellationToken cancellationToken)
    {
        return olvaCourierClient.GetDistrictsAsync(request.Department, request.Province, cancellationToken);
    }
}

internal sealed class QuoteOlvaShipmentQueryHandler(IOlvaCourierClient olvaCourierClient)
    : IQueryHandler<QuoteOlvaShipmentQuery, OlvaQuoteResponse>
{
    public Task<Result<OlvaQuoteResponse>> Handle(QuoteOlvaShipmentQuery request, CancellationToken cancellationToken)
    {
        return olvaCourierClient.QuoteAsync(
            new OlvaQuoteRequest(
                request.Origin,
                request.Destination,
                request.DeliveryType,
                request.ShipmentType,
                request.Weight,
                request.PartnerRate,
                request.Length,
                request.Width,
                request.Height),
            cancellationToken);
    }
}

internal sealed class TrackOlvaShipmentQueryHandler(IOlvaCourierClient olvaCourierClient)
    : IQueryHandler<TrackOlvaShipmentQuery, OlvaTrackingResponse>
{
    public Task<Result<OlvaTrackingResponse>> Handle(TrackOlvaShipmentQuery request, CancellationToken cancellationToken)
    {
        return olvaCourierClient.TrackAsync(request.OrderNumber, request.OrderCode, cancellationToken);
    }
}

internal static class OlvaAgencyMappings
{
    public static CachedOlvaAgencyResponse ToCachedAgencyResponse(OlvaAgencyEntity agency)
    {
        return new CachedOlvaAgencyResponse(
            agency.Id,
            agency.Code,
            agency.Name,
            agency.Type,
            agency.Department,
            agency.Province,
            agency.District,
            agency.Address,
            agency.Phone,
            agency.ScheduleJson,
            agency.Latitude,
            agency.Longitude,
            agency.LastSyncedAt);
    }
}

internal static class OlvaLocationNames
{
    public static string Normalize(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalized.StartsWith("CERCADO DE ", StringComparison.Ordinal)
            ? normalized["CERCADO DE ".Length..].Trim()
            : normalized;
    }

    public static bool Matches(string? actual, string? requested)
        => string.IsNullOrWhiteSpace(requested) || Normalize(actual) == Normalize(requested);

    public static bool IsValid(string? value)
    {
        var normalized = value?.Trim();
        return !string.IsNullOrWhiteSpace(normalized) && !string.Equals(normalized, "string", StringComparison.OrdinalIgnoreCase);
    }
}
