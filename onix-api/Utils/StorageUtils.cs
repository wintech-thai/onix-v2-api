using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;

namespace Its.Onix.Api.Utils
{
    public interface IStorageUtils
    {
        string GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null);
    }

    public class StorageUtils : IStorageUtils
    {
        private readonly UrlSigner _urlSigner;

        public StorageUtils(GoogleCredential credential)
        {
            // สร้าง UrlSigner จาก service account credential
            _urlSigner = UrlSigner.FromCredential(
                credential.UnderlyingCredential as ServiceAccountCredential 
                ?? throw new InvalidOperationException("Expected service account credential.")
            );
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
    }
}
