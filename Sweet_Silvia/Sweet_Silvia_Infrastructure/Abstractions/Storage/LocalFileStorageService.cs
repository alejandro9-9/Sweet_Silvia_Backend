using Sweet_Silvia_Application.Abstractions.Storage;

namespace Sweet_Silvia_Infrastructure.Abstractions.Storage;

internal sealed class LocalFileStorageService(FileStorageOptions options) : IFileStorageService
{
    public async Task<FileStorageResult> SaveAsync(FileStorageUpload upload, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(upload.OriginalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var container = NormalizeContainer(upload.Container);
        var rootPath = Path.GetFullPath(upload.IsPublic ? options.LocalRootPath : options.PrivateLocalRootPath);
        var directory = Path.Combine(rootPath, container);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, fileName);
        await using (var fileStream = File.Create(fullPath))
        {
            await upload.Content.CopyToAsync(fileStream, cancellationToken);
        }

        var key = $"{container}/{fileName}".Replace("\\", "/", StringComparison.Ordinal);
        var url = upload.IsPublic
            ? string.IsNullOrWhiteSpace(options.PublicBaseUrl)
                ? $"/uploads/{key}"
                : $"{options.PublicBaseUrl.TrimEnd('/')}/uploads/{key}"
            : key;

        return new FileStorageResult(url, key);
    }

    public Task DeleteAsync(string keyOrUrl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var rootPath in new[] { options.LocalRootPath, options.PrivateLocalRootPath })
        {
            if (TryResolvePath(keyOrUrl, rootPath, out var fullPath) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
                break;
            }
        }

        return Task.CompletedTask;
    }

    public async Task<FileStorageReadResult?> OpenPrivateReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!TryResolvePrivatePath(key, out var fullPath))
        {
            return null;
        }

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
        return new FileStorageReadResult(stream, GetContentType(fullPath));
    }

    private bool TryResolvePrivatePath(string key, out string fullPath)
        => TryResolvePath(key, options.PrivateLocalRootPath, out fullPath);

    private static bool TryResolvePath(string key, string rootPath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalizedKey = ExtractKey(key).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var segments = normalizedKey.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        if (Path.IsPathRooted(normalizedKey) || segments.Any(segment => segment is "." or ".."))
        {
            return false;
        }

        var root = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        fullPath = Path.GetFullPath(Path.Combine(root, normalizedKey));
        return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractKey(string key)
    {
        var trimmed = key.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            trimmed = uri.AbsolutePath;
        }

        const string uploadsPrefix = "/uploads/";
        var uploadsIndex = trimmed.IndexOf(uploadsPrefix, StringComparison.OrdinalIgnoreCase);
        if (uploadsIndex >= 0)
        {
            trimmed = trimmed[(uploadsIndex + uploadsPrefix.Length)..];
        }

        return trimmed.TrimStart('/', '\\');
    }

    private static string GetContentType(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

    private static string NormalizeContainer(string container)
    {
        if (string.IsNullOrWhiteSpace(container))
        {
            throw new ArgumentException("Storage container is required.", nameof(container));
        }

        return container
            .Trim()
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(Path.Combine);
    }
}
