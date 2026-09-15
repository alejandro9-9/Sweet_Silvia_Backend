using Amazon.S3;
using Amazon.S3.Model;
using Sweet_Silvia_Application.Abstractions.Storage;

namespace Sweet_Silvia_Infrastructure.Abstractions.Storage;

internal sealed class S3FileStorageService(IAmazonS3 s3Client, FileStorageOptions options) : IFileStorageService
{
    public async Task<FileStorageResult> SaveAsync(FileStorageUpload upload, CancellationToken cancellationToken = default)
    {
        var bucketName = GetBucketName(upload.IsPublic);
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            var setting = upload.IsPublic ? "S3PublicBucketName" : "S3PrivateBucketName";
            throw new InvalidOperationException($"FileStorage:{setting} is required when FileStorage:Provider is S3.");
        }

        var extension = Path.GetExtension(upload.OriginalFileName).ToLowerInvariant();
        var key = $"{NormalizeContainer(upload.Container)}/{Guid.NewGuid():N}{extension}";

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = upload.Content,
            ContentType = upload.ContentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        var url = upload.IsPublic
            ? string.IsNullOrWhiteSpace(options.S3PublicBaseUrl)
                ? $"https://{bucketName}.s3.{options.S3Region}.amazonaws.com/{key}"
                : $"{options.S3PublicBaseUrl.TrimEnd('/')}/{key}"
            : key;

        return new FileStorageResult(url, key);
    }

    public async Task DeleteAsync(string keyOrUrl, CancellationToken cancellationToken = default)
    {
        if (!TryExtractKey(keyOrUrl, out var key))
        {
            return;
        }

        var isPublic = Uri.TryCreate(keyOrUrl.Trim(), UriKind.Absolute, out _);
        var bucketName = GetBucketName(isPublic);
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return;
        }

        await s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key
        }, cancellationToken);
    }

    public async Task<FileStorageReadResult?> OpenPrivateReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsSafeKey(key))
        {
            return null;
        }

        try
        {
            using var response = await s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = GetBucketName(isPublic: false),
                Key = key.Trim().Replace('\\', '/')
            }, cancellationToken);

            var content = new MemoryStream();
            await response.ResponseStream.CopyToAsync(content, cancellationToken);
            content.Position = 0;
            return new FileStorageReadResult(content, string.IsNullOrWhiteSpace(response.Headers.ContentType)
                ? "application/octet-stream"
                : response.Headers.ContentType);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private string GetBucketName(bool isPublic)
    {
        var configuredBucket = isPublic ? options.S3PublicBucketName : options.S3PrivateBucketName;
        return string.IsNullOrWhiteSpace(configuredBucket) ? options.S3BucketName : configuredBucket;
    }

    private static string NormalizeContainer(string container)
    {
        if (string.IsNullOrWhiteSpace(container))
        {
            throw new ArgumentException("Storage container is required.", nameof(container));
        }

        return string.Join("/", container
            .Trim()
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsSafeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalizedKey = key.Trim().Replace('\\', '/');
        return !normalizedKey.StartsWith('/') &&
               normalizedKey.Split('/', StringSplitOptions.RemoveEmptyEntries).All(segment => segment is not "." and not "..");
    }

    private static bool TryExtractKey(string keyOrUrl, out string key)
    {
        key = string.Empty;
        if (string.IsNullOrWhiteSpace(keyOrUrl))
        {
            return false;
        }

        var trimmed = keyOrUrl.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            trimmed = uri.AbsolutePath;
        }

        key = trimmed.TrimStart('/', '\\').Replace('\\', '/');
        return IsSafeKey(key);
    }
}
