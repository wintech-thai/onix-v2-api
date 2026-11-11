using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IConfigurationService
    {
        public Task<MVConfiguration?> GetBrandConfig(string orgId, bool needDownloadUrl = false);
        public Task<MVConfiguration> SetBrandConfig(string orgId, MConfiguration config);
        public Task<MVConfiguration?> SetConfigStatusById(string orgId, string configId, string status);

        [Obsolete("Brand logo is now uploaded as base64 directly via SetBrandConfig. Kept only until legacy clients stop calling it.")]
        public Task<MVPresignedUrl> GetBrandLogoUploadPresignedUrl(string orgId, VMUploadDocument param);

        public Task<(byte[]? Bytes, string? MimeType)> GetBrandLogoImageBytes(string orgId);
        public Task<MVConfiguration?> GetBackupPolicy(string orgId);
        public Task<MVConfiguration> SetBackupPolicy(string orgId, MConfiguration config);
        public Task<MVConfiguration?> GetClientIpSource(string orgId, string scope);
        public Task<MVConfiguration> SetClientIpSource(string orgId, string scope, MConfiguration config);
        public Task<MVConfiguration?> GetSmtpConfig(string orgId);
        public Task<MVConfiguration> SetSmtpConfig(string orgId, MConfiguration config);

        // x093 4/5 : key ที่ merchant เอาไปใส่ header PP-API-KEY ของ webhook ตัวเอง, ใช้ authen
        // ตอน PaymentConfirmationController (anonymous) ถูกเรียกจาก Ruby script
        public Task<MVConfiguration?> GetPaymentConfirmationConfig(string orgId);
        public Task<MVConfiguration> SetPaymentConfirmationConfig(string orgId, MConfiguration config);
        public Task<string?> GetSystemPaymentConfirmationKeyCached(string orgId);
    }
}
