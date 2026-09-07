namespace Sweet_Silvia_Application.Abstractions.Storage;

public sealed record FileStorageReadResult(Stream Content, string ContentType);
