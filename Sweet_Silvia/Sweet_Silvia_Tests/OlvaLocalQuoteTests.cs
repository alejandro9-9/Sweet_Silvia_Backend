using Sweet_Silvia_Application.Olva;
using Sweet_Silvia_Infrastructure.Abstractions.Shipping;

namespace Sweet_Silvia_Tests;

public sealed class OlvaLocalQuoteTests
{
    [Fact]
    public async Task Missing_api_key_returns_local_estimate_only_in_development()
    {
        var handler = new NonSendingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.olva-api.lat/")
        };
        var client = new OlvaCourierClient(
            httpClient,
            new OlvaOptions { UseLocalQuoteEstimateWhenUnconfigured = true });

        var result = await client.QuoteAsync(CreateQuoteRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(22m, result.Value.Data.GetProperty("cost").GetDecimal());
        Assert.True(result.Value.Data.GetProperty("isLocalEstimate").GetBoolean());
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task Missing_api_key_returns_configuration_error_when_estimate_is_disabled()
    {
        var handler = new NonSendingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.olva-api.lat/")
        };
        var client = new OlvaCourierClient(
            httpClient,
            new OlvaOptions());

        var result = await client.QuoteAsync(CreateQuoteRequest());

        Assert.True(result.IsFailure);
        Assert.Equal("Olva.ApiKeyMissing", result.Error.Code);
        Assert.False(handler.WasCalled);
    }

    private static OlvaQuoteRequest CreateQuoteRequest()
        => new("150136", "090101", "O", 2, 1.32m, false, 30m, 22m, 12m);

    private sealed class NonSendingHandler : HttpMessageHandler
    {
        public bool WasCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            throw new InvalidOperationException("The local quote test must not call Olva.");
        }
    }

}
