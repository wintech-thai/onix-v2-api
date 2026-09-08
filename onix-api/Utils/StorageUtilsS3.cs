using System.Net.Http.Headers;
using Minio;
using Minio.DataModel.Args;

namespace Its.Onix.Api.Utils
{
    public class StorageUtilsS3 : IStorageUtilsS3
    {
        private readonly IMinioClient _minioClient;

        public StorageUtilsS3(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }


        public async Task<string> GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            var args = new PresignedPutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry((int) validFor.TotalSeconds);

            return await _minioClient.PresignedPutObjectAsync(args);
        }

        public async Task<string> GenerateDownloadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry((int) validFor.TotalSeconds);

            return await _minioClient.PresignedGetObjectAsync(args);
        }

        // Used for one-time migration of legacy MinIO-hosted files (e.g. brand logo) into DB-stored base64 content.
        public async Task<byte[]?> DownloadObjectAsync(string bucketName, string objectName)
        {
            try
            {
                using var ms = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyTo(ms));

                await _minioClient.GetObjectAsync(args);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - DownloadObjectAsync [{bucketName}/{objectName}] - [{ex.Message}]");
                return null;
            }
        }
    }
}
