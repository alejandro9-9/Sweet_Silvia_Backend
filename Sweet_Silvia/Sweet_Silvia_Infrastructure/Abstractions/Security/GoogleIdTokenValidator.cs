using Google.Apis.Auth;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Sweet_Silvia_Application.Abstractions.Security;

namespace Sweet_Silvia_Infrastructure.Abstractions.Security;

internal sealed class GoogleIdTokenValidator(HttpClient httpClient, GoogleAuthOptions options) : IGoogleIdTokenValidator
{
    public bool IsConfigured => options.Enabled && !string.IsNullOrWhiteSpace(options.ClientId);

    public async Task<GoogleAccountInfo?> ValidateAsync(
        string? idToken,
        string? authorizationCode,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || (string.IsNullOrWhiteSpace(idToken) && string.IsNullOrWhiteSpace(authorizationCode)))
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                if (string.IsNullOrWhiteSpace(options.ClientSecret) || string.IsNullOrWhiteSpace(options.RedirectUri))
                {
                    return null;
                }

                var tokenResponse = await ExchangeAuthorizationCodeAsync(authorizationCode!, cancellationToken);
                idToken = tokenResponse?.IdToken;
            }

            if (string.IsNullOrWhiteSpace(idToken))
            {
                return null;
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [options.ClientId]
                });

            if (!IsGoogleIssuer(payload.Issuer) ||
                string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email) ||
                !payload.EmailVerified)
            {
                return null;
            }

            return new GoogleAccountInfo(
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.Name,
                payload.GivenName,
                payload.FamilyName);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private async Task<GoogleTokenResponse?> ExchangeAuthorizationCodeAsync(
        string authorizationCode,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = authorizationCode,
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret,
            ["redirect_uri"] = options.RedirectUri,
            ["grant_type"] = "authorization_code"
        });

        using var response = await httpClient.PostAsync("token", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);
    }

    private static bool IsGoogleIssuer(string? issuer) =>
        string.Equals(issuer, "accounts.google.com", StringComparison.Ordinal) ||
        string.Equals(issuer, "https://accounts.google.com", StringComparison.Ordinal);

    private sealed record GoogleTokenResponse(
        [property: JsonPropertyName("id_token")] string? IdToken,
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string? TokenType);
}
