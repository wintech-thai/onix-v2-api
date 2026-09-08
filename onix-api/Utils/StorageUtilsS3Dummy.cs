namespace Its.Onix.Api.Utils
{
    // ใช้ตอน MINIO_ENDPOINT ไม่ได้ตั้งค่าไว้ (เช่น deployment ที่ไม่ได้ใช้ MinIO เลย เช่น Please Scan)
    // กัน DI resolve IMinioClient/StorageUtilsS3 ตัวจริงแล้ว throw ตอน startup
    public class StorageUtilsS3Dummy : IStorageUtilsS3
    {
        public Task<string> GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            return Task.FromResult("");
        }

        public Task<string> GenerateDownloadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            return Task.FromResult("");
        }

        public Task<byte[]?> DownloadObjectAsync(string bucketName, string objectName)
        {
            return Task.FromResult<byte[]?>(null);
        }
    }
}
