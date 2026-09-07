namespace Sweet_Silvia_Infrastructure.Abstractions.Security;

public sealed class GoogleAuthOptions
{
    public bool Enabled { get; init; }

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string RedirectUri { get; init; } = string.Empty;
}
