using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Common;

internal static class ApplicationErrors
{
    public static Error NotFound(string resource, Guid id) =>
        new($"{resource}.NotFound", $"{resource} with id '{id}' was not found.");

    public static Error NotFound(string resource) =>
        new($"{resource}.NotFound", $"{resource} was not found.");

    public static Error Conflict(string code, string message) =>
        new(code, message);

    public static Error Unauthorized =>
        new("Auth.Unauthorized", "Invalid credentials.");
}

