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
        public Task<MVPresignedUrl> GetBrandLogoUploadPresignedUrl(string orgId, VMUploadDocument param);
        public Task<MVConfiguration?> GetBackupPolicy(string orgId);
        public Task<MVConfiguration> SetBackupPolicy(string orgId, MConfiguration config);
        public Task<MVConfiguration?> GetClientIpSource(string orgId, string scope);
        public Task<MVConfiguration> SetClientIpSource(string orgId, string scope, MConfiguration config);
    }
}
