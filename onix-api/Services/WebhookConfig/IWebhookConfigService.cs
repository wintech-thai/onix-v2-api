using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IWebhookConfigService
    {
        public Task<MVWebhookConfig> GetWebhookConfigById(string orgId, string webhookConfigId);
        public Task<MVWebhookConfig> DeleteWebhookById(string orgId, string webhookConfigId);
        public Task<MVWebhookConfig> AddMerchantWebhookConfig(string orgId, string merchantId, MWebhookConfig webhookConfig);
        public Task<List<MWebhookConfig>> GetWebhookConfigsByMerchantId(string orgId, string merchantId);
        public Task<int> GetWebhookConfigsCountByMerchantId(string orgId, string merchantId);
        public Task<MVWebhookConfig> UpdateWebhookConfigById(string orgId, string webhookConfigId, MWebhookConfig webhookConfig);
        public Task<MVWebhookConfig> UpdateWebhookConfigStatusById(string orgId, string webhookConfigId, bool isEnable);
    }
}
