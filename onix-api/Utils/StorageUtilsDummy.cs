
namespace Its.Onix.Api.Utils
{
    public class StorageUtilsDummy : IStorageUtils
    {
        public StorageUtilsDummy()
        {
        }

        public string GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            return "";
        }

        public void UpdateMetaData(string bucketName, string objectName, string metaName, string metaValue)
        {
        }

        public async Task<byte[]> PartialDownloadToStream(string bucketName, string objectName, long start, long end)
        {
            await Task.CompletedTask;
            var data = Array.Empty<byte>();
            return data;
        }

        public void DeleteObject(string bucketName, string objectName)
        {
        }

        public Google.Apis.Storage.v1.Data.Object? GetStorageObject(string bucketName, string objectName)
        {
            return null;
        }

        public bool IsObjectExist(string objectName)
        {
            return false;
        }

        public string GenerateDownloadUrl(string objectName, TimeSpan validFor, string? contentType = null)
        {
            return "";
        }
    }
}
