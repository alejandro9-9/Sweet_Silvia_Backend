namespace Sweet_Silvia_Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveAsync(FileStorageUpload upload, CancellationToken cancellationToken = default);
}
