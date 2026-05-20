using LinqKit;
using Its.Onix.Api.Models;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class WebhookConfigRepository : BaseRepository, IWebhookConfigRepository
    {
        public WebhookConfigRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsEventNameExist(string name)
        {
            var exists = await context!.WebhookConfigs!.AsExpandable()
                .AnyAsync(p => p!.EventName!.Equals(name) && p!.OrgId!.Equals(orgId));

            return exists;
        }

        public async Task<MWebhookConfig> GetWebhookConfigByEventName(string eventName)
        {
            var u = await GetSelection().AsExpandable()
                .Where(p => p!.EventName!.Equals(eventName) && p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();

            return u;
        }

        public IQueryable<MWebhookConfig> GetSelection()
        {
            var query =
                from wc in context!.WebhookConfigs

                select new { wc };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MWebhookConfig
            {
                WebhookId = x.wc.WebhookId,
                OrgId = x.wc.OrgId,
                MerchantId = x.wc.MerchantId,
                EventName = x.wc.EventName,
                Description = x.wc.Description,
                EndpointUrl = x.wc.EndpointUrl,
                HttpMethod = x.wc.HttpMethod,
                IsActive = x.wc.IsActive,
                SecretKey = x.wc.SecretKey,
                SignatureAlgorithm = x.wc.SignatureAlgorithm,
                HeadersDefinition = x.wc.HeadersDefinition,
                TimeoutSec = x.wc.TimeoutSec,
                MaxRetryCount = x.wc.MaxRetryCount,
                RetryIntervalSec = x.wc.RetryIntervalSec,
                PayloadVersion = x.wc.PayloadVersion,
                LastStatus = x.wc.LastStatus,
                LastCalledDate = x.wc.LastCalledDate,
                CreatedDate = x.wc.CreatedDate,
            });
        }

        public async Task<List<MWebhookConfig>> GetWebhooksByMerchantId(string merchantId)
        {
            var id = Guid.Parse(merchantId);

            var result = await GetSelection().AsExpandable()
                .Where(p => p.MerchantId == id)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<int> GetWebhookCountByMerchantId(string merchantId)
        {
            var id = Guid.Parse(merchantId);

            var result = await GetSelection()
                .Where(p => p.MerchantId == id)
                .AsExpandable()
                .CountAsync();

            return result;
        }

        public async Task<MWebhookConfig?> DeleteWebhookConfigById(string webhookConfigId)
        {
            Guid id = Guid.Parse(webhookConfigId);
            var existing = await context!.WebhookConfigs!.AsExpandable()
            .Where(p => p!.WebhookId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            
            if (existing != null)
            {
                context.WebhookConfigs!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MWebhookConfig?> GetWebhookConfigById(string webhookConfigId)
        {
            Guid id = Guid.Parse(webhookConfigId);

            var u = await GetSelection().AsExpandable()
                .Where(p => p!.WebhookId!.Equals(id) && p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();

            return u;
        }

        public async Task<MWebhookConfig?> UpdateWebhookConfigById(string webhookConfigId, MWebhookConfig webhookConfig)
        {
            Guid id = Guid.Parse(webhookConfigId);
            var existing = await context!.WebhookConfigs!.AsExpandable().Where(p => p!.WebhookId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Description = webhookConfig.Description;
                existing.EventName = webhookConfig.EventName;
                existing.EndpointUrl = webhookConfig.EndpointUrl;
                existing.HttpMethod = webhookConfig.HttpMethod;
                existing.IsActive = webhookConfig.IsActive;
                existing.SecretKey = webhookConfig.SecretKey;
                existing.SignatureAlgorithm = webhookConfig.SignatureAlgorithm;
                existing.HeadersDefinition = webhookConfig.HeadersDefinition;
                existing.TimeoutSec = webhookConfig.TimeoutSec;
                existing.MaxRetryCount = webhookConfig.MaxRetryCount;
                existing.RetryIntervalSec = webhookConfig.RetryIntervalSec;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MWebhookConfig> AddMerchantWebhookConfig(string merchantId, MWebhookConfig webhookConfig)
        {
            Guid id = Guid.Parse(merchantId);

            webhookConfig.OrgId = orgId;
            webhookConfig.CreatedDate = DateTime.UtcNow;
            webhookConfig.MerchantId = id;

            await context!.WebhookConfigs!.AddAsync(webhookConfig);
            await context.SaveChangesAsync();

            return webhookConfig;
        }
    }
}