namespace Sweet_Silvia_Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveAsync(FileStorageUpload upload, CancellationToken cancellationToken = default);
    Task DeleteAsync(string keyOrUrl, CancellationToken cancellationToken = default);
    Task<FileStorageReadResult?> OpenPrivateReadAsync(string key, CancellationToken cancellationToken = default);
}
