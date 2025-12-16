using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Simpchat.Application.Interfaces.File;
using Simpchat.Shared.Config;

namespace Simpchat.Infrastructure.FileStorage
{
    internal class FileStorageService : IFileStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _minioSettings;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IMinioClient minioClient, MinioSettings minioSettings, ILogger<FileStorageService> logger)
        {
            _minioClient = minioClient;
            _minioSettings = minioSettings;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream data, string contentType)
        {
            try
            {
                bool found = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName)
                ).ConfigureAwait(false);

                if (!found)
                {
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(bucketName)
                    ).ConfigureAwait(false);

                    // Make bucket public for file downloads
                    await MakeBucketPublicAsync(bucketName).ConfigureAwait(false);
                }

                await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(data)
                        .WithObjectSize(data.Length)
                        .WithContentType(contentType)
                ).ConfigureAwait(false);

                // Return permanent public URL (no expiry)
                return GetPublicUrl(bucketName, objectName);
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Upload Error: {Message}", e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[General] Error during upload: {Message}", e.Message);
                throw;
            }
        }

        public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 604800)
        {
            try
            {
                var presignedUrl = await _minioClient.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithExpiry(expiryInSeconds)
                ).ConfigureAwait(false);

                return presignedUrl;
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Presigned URL Error: {Message}", e.Message);
                throw;
            }
        }

        public string GetPublicUrl(string bucketName, string objectName)
        {
            // Use PublicEndpoint for browser-accessible URLs, fallback to Endpoint if not set
            var endpoint = !string.IsNullOrEmpty(_minioSettings.PublicEndpoint)
                ? _minioSettings.PublicEndpoint
                : _minioSettings.Endpoint;

            var protocol = _minioSettings.UseSsl ? "https" : "http";
            var url = $"{protocol}://{endpoint}/{bucketName}/{objectName}";
            _logger.LogInformation("[Minio] Generated public URL: {Url} (endpoint: {Endpoint})", url, endpoint);
            return url;
        }

        public async Task<MemoryStream> DownloadFileAsync(string bucketName, string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                await _minioClient.GetObjectAsync(
                    new GetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithCallbackStream(async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                        })
                ).ConfigureAwait(false);

                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Download Error: {Message}", e.Message);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string bucketName, string objectName)
        {
            try
            {
                await _minioClient.StatObjectAsync(
                    new StatObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                ).ConfigureAwait(false);
                return true;
            }
            catch (MinioException e) when (e.Message.Contains("Object does not exist"))
            {
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveFileAsync(string bucketName, string objectName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                ).ConfigureAwait(false);
                return true;
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Remove Error: {Message}", e.Message);
                throw;
            }
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                return await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName)
                ).ConfigureAwait(false);
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Bucket Check Error: {Message}", e.Message);
                throw;
            }
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                bool found = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName)
                ).ConfigureAwait(false);

                if (!found)
                {
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(bucketName)
                    ).ConfigureAwait(false);
                }
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Create Bucket Error: {Message}", e.Message);
                throw;
            }
        }

        public async Task SetBucketPolicyAsync(string bucketName, string policy)
        {
            try
            {
                await _minioClient.SetPolicyAsync(
                    new SetPolicyArgs()
                        .WithBucket(bucketName)
                        .WithPolicy(policy)
                ).ConfigureAwait(false);

                _logger.LogInformation("[Minio] Bucket policy set for {BucketName}", bucketName);
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "[Minio] Set Policy Error: {Message}", e.Message);
                throw;
            }
        }

        public async Task MakeBucketPublicAsync(string bucketName)
        {
            try
            {
                // Create a policy that allows public read access
                var policy = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{""AWS"": [""*""]}},
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                        }}
                    ]
                }}";

                await SetBucketPolicyAsync(bucketName, policy).ConfigureAwait(false);
                _logger.LogInformation("[Minio] Bucket {BucketName} is now public", bucketName);
            }
            catch (MinioException e)
            {
                _logger.LogWarning(e, "[Minio] Failed to make bucket public (continuing with presigned URLs): {Message}", e.Message);
                // Don't throw - we'll use presigned URLs instead
            }
        }
    }
}
