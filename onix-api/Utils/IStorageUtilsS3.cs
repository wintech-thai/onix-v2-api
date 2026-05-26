namespace Its.Onix.Api.Utils
{
    public interface IStorageUtilsS3
    {
        public Task<string> GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null);
        public Task<string> GenerateDownloadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null);
/*
        bool IsObjectExist(string objectName);
        public void UpdateMetaData(string bucketName, string objectName, string metaName, string metaValue);
        public void DeleteObject(string bucketName, string objectName);
        public Task<byte[]> PartialDownloadToStream(string bucketName, string objectName, long start, long end);
*/
        //public Google.Apis.Storage.v1.Data.Object? GetStorageObject(string bucketName, string objectName);
    }
}