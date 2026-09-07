namespace Sweet_Silvia_Application.Abstractions.Storage;

public sealed record FileStorageUpload(
    string Container,
    string OriginalFileName,
    string ContentType,
    Stream Content,
    bool IsPublic = true);
