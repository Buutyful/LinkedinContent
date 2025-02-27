using Microsoft.Extensions.Options;
using Minio.DataModel.Args;
using Minio;

namespace VetrinaGalaApp.ApiService.Infrastructure.MinIo;

public interface IMinioService
{
    Task<string> GeneratePresignedPutUrl(string objectKey, int expirySeconds = 3600);
    Task<string> GetPublicObjectUrl(string objectKey);
    Task EnsureBucketExists();
}

public class MinioService(IMinioClient minioClient, IOptions<MinioSettings> settings) : IMinioService
{
    private readonly IMinioClient _minioClient = minioClient;
    private readonly MinioSettings _settings = settings.Value;

    public async Task<string> GeneratePresignedPutUrl(string objectKey, int expirySeconds = 3600)
    {
        await EnsureBucketExists();

        return await _minioClient.PresignedPutObjectAsync(new PresignedPutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectKey)
            .WithExpiry(expirySeconds));
    }

    public async Task<string> GetPublicObjectUrl(string objectKey)
    {
        await EnsureBucketExists();
        return $"{_settings.Endpoint}/{_settings.BucketName}/{objectKey}";
    }

    public async Task EnsureBucketExists()
    {
        bool bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_settings.BucketName));

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_settings.BucketName));

            // Set bucket policy to make objects publicly readable
            var policy = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Principal"": {{""AWS"": [""*""]}},
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{_settings.BucketName}/*""]
                    }}
                ]
            }}";

            await _minioClient.SetPolicyAsync(
                new SetPolicyArgs().WithBucket(_settings.BucketName).WithPolicy(policy));
        }
    }
}