using Sweet_Silvia_Application.Abstractions.Storage;

namespace Sweet_Silvia_Infrastructure.Abstractions.Storage;

internal sealed class LocalFileStorageService(FileStorageOptions options) : IFileStorageService
{
    public async Task<FileStorageResult> SaveAsync(FileStorageUpload upload, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(upload.OriginalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var container = NormalizeContainer(upload.Container);
        var rootPath = Path.GetFullPath(options.LocalRootPath);
        var directory = Path.Combine(rootPath, container);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, fileName);
        await using (var fileStream = File.Create(fullPath))
        {
            await upload.Content.CopyToAsync(fileStream, cancellationToken);
        }

        var key = $"{container}/{fileName}".Replace("\\", "/", StringComparison.Ordinal);
        var url = string.IsNullOrWhiteSpace(options.PublicBaseUrl)
            ? $"/uploads/{key}"
            : $"{options.PublicBaseUrl.TrimEnd('/')}/uploads/{key}";

        return new FileStorageResult(url, key);
    }

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
