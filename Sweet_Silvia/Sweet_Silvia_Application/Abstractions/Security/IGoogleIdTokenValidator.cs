namespace Sweet_Silvia_Application.Abstractions.Security;

public interface IGoogleIdTokenValidator
{
    bool IsConfigured { get; }

    Task<GoogleAccountInfo?> ValidateAsync(
        string? idToken,
        string? authorizationCode,
        CancellationToken cancellationToken = default);
}

public sealed record GoogleAccountInfo(
    string Subject,
    string Email,
    bool EmailVerified,
    string? Name,
    string? GivenName,
    string? FamilyName);
