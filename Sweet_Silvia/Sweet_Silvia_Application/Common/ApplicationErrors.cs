using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Common;

internal static class ApplicationErrors
{
    public static Error NotFound(string resource, Guid id) =>
        new($"{resource}.NotFound", $"{resource} with id '{id}' was not found.") { StatusCode = 404 };

    public static Error NotFound(string resource) =>
        new($"{resource}.NotFound", $"{resource} was not found.") { StatusCode = 404 };

    public static Error Conflict(string code, string message) =>
        new(code, message) { StatusCode = 409 };

    public static Error Validation(string code, string message) =>
        new(code, message) { StatusCode = 400 };

    public static Error Unauthorized =>
        new("Auth.Unauthorized", "Invalid credentials.") { StatusCode = 401 };

    public static Error InvalidCredentials =>
        new("Auth.InvalidCredentials", "The email or password is incorrect.") { StatusCode = 401 };

    public static Error UserBlocked =>
        new("Auth.UserBlocked", "This account is blocked.") { StatusCode = 403 };

    public static Error UserInactive =>
        new("Auth.UserInactive", "This account is inactive.") { StatusCode = 403 };
}
