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
    }
}
