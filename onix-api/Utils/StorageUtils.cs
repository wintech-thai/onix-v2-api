using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;

namespace Its.Onix.Api.Utils
{
    public interface IStorageUtils
    {
        string GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null);
        bool IsObjectExist(string objectName);
        public string GenerateDownloadUrl(string objectName, TimeSpan validFor, string? contentType = null);
    }

    public class StorageUtils : IStorageUtils
    {
        private readonly UrlSigner _urlSigner;
        private readonly StorageClient _storageClient;

        public StorageUtils(GoogleCredential credential, StorageClient storageClient)
        {
            // สร้าง UrlSigner จาก service account credential
            _urlSigner = UrlSigner.FromCredential(
                credential.UnderlyingCredential as ServiceAccountCredential
                ?? throw new InvalidOperationException("Expected service account credential.")
            );

            _storageClient = storageClient;
        }

        public string GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            var options = UrlSigner.Options.FromDuration(validFor);

            var template = UrlSigner.RequestTemplate
                .FromBucket(bucketName)
                .WithObjectName(objectName)
                .WithHttpMethod(HttpMethod.Put);

            if (!string.IsNullOrEmpty(contentType))
            {
                template = template.WithContentHeaders(
                [
                    new KeyValuePair<string, IEnumerable<string>>("Content-Type", [contentType])
                ]);
            }

            return _urlSigner.Sign(template, options);
        }

        public bool IsObjectExist(string objectName)
        {
            var bucketName = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;

            try
            {
                var obj = _storageClient.GetObject(bucketName, objectName);
                return obj != null;  // ถ้าเจอจะไม่เป็น null
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false; // ไม่พบ object
            }
        }

        public string GenerateDownloadUrl(string objectName, TimeSpan validFor, string? contentType = null)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return "";
            }
 
            var bucketName = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
/*
            var options = UrlSigner.Options.FromDuration(validFor);

            var template = UrlSigner.RequestTemplate
                .FromBucket(bucketName)
                .WithObjectName(objectName)
                .WithHttpMethod(HttpMethod.Get);

            if (!string.IsNullOrEmpty(contentType))
            {
                template = template.WithContentHeaders(
                [
                    new KeyValuePair<string, IEnumerable<string>>("Content-Type", [contentType])
                ]);
            }

            //return _urlSigner.Sign(template, options);
*/
            return _urlSigner.Sign(bucketName, objectName, validFor, HttpMethod.Get);
        }
    }
}
