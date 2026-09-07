namespace Sweet_Silvia_Infrastructure.Abstractions.Storage;

public sealed class FileStorageOptions
{
    public string Provider { get; init; } = "Local";
    public string LocalRootPath { get; init; } = "wwwroot/uploads";
    public string PrivateLocalRootPath { get; init; } = "App_Data/private-uploads";
    public string PublicBaseUrl { get; init; } = string.Empty;
    public string S3BucketName { get; init; } = string.Empty;
    public string S3Region { get; init; } = "us-east-1";
    public string S3PublicBaseUrl { get; init; } = string.Empty;
}
