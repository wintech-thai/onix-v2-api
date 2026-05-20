using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IWebhookConfigRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsEventNameExist(string name);
        public Task<List<MWebhookConfig>> GetWebhooksByMerchantId(string merchantId);
        public Task<MWebhookConfig> GetWebhookConfigByEventName(string eventName);
        public Task<int> GetWebhookCountByMerchantId(string merchantId);
        public Task<MWebhookConfig?> DeleteWebhookConfigById(string webhookConfigId);
        public Task<MWebhookConfig?> GetWebhookConfigById(string webhookConfigId);
        public Task<MWebhookConfig?> UpdateWebhookConfigById(string webhookConfigId, MWebhookConfig webhookConfig);
        public Task<MWebhookConfig> AddMerchantWebhookConfig(string merchantId, MWebhookConfig webhookConfig);
    }
}
