using Backend.Common.Configuration;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Backend.Common.Services;

public class PublicUrlGenerator : IPublicUrlGenerator
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _minioOptions;

    public PublicUrlGenerator(IMinioClient minioClient, IOptions<MinioOptions> minioOptions)
    {
        _minioClient = minioClient;
        _minioOptions = minioOptions.Value;
    }

    public async Task<string> GenerateUrlAsync(string bucket, string path)
    {
        try
        {
            // Verify bucket exists
            var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
            if (!bucketExists)
            {
                throw new InvalidOperationException($"Bucket '{bucket}' does not exist");
            }

            // Verify object exists
            try
            {
                await _minioClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(path));
            }
            catch (ObjectNotFoundException)
            {
                throw new FileNotFoundException($"Object '{path}' not found in bucket '{bucket}'");
            }

            // Generate public URL - direct access URL that never expires
            var protocol = _minioOptions.Endpoint.StartsWith("https://") ? "https" : "http";
            var endpoint = _minioOptions.Endpoint.Replace("https://", "").Replace("http://", "");

            // Clean up path - remove leading slash if present
            var cleanPath = path.StartsWith("/") ? path.Substring(1) : path;

            return $"{protocol}://{endpoint}/{bucket}/{cleanPath}";
        }
        catch (MinioException ex)
        {
            throw new InvalidOperationException(
                $"MinIO error while generating URL for bucket '{bucket}', path '{path}': {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException || ex is FileNotFoundException))
        {
            throw new InvalidOperationException(
                $"Unexpected error while generating URL for bucket '{bucket}', path '{path}': {ex.Message}", ex);
        }
    }
}