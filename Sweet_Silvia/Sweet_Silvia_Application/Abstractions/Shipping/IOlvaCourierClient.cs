using Sweet_Silvia_Application.Olva;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Abstractions.Shipping;

public interface IOlvaCourierClient
{
    Task<Result<OlvaValidationResponse>> ValidateAsync(CancellationToken cancellationToken = default);

    Task<Result<List<OlvaAgencyResponse>>> GetAgenciesAsync(
        string? q,
        string? department,
        string? province,
        string? district,
        CancellationToken cancellationToken = default);

    Task<Result<List<string>>> GetDepartmentsAsync(CancellationToken cancellationToken = default);

    Task<Result<List<string>>> GetProvincesAsync(string department, CancellationToken cancellationToken = default);

    Task<Result<List<string>>> GetDistrictsAsync(string department, string province, CancellationToken cancellationToken = default);

    Task<Result<OlvaQuoteResponse>> QuoteAsync(OlvaQuoteRequest request, CancellationToken cancellationToken = default);

    Task<Result<OlvaTrackingResponse>> TrackAsync(string orderNumber, string? orderCode, CancellationToken cancellationToken = default);
}
