using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class WebhookConfigService : BaseService, IWebhookConfigService
    {
        private readonly IWebhookConfigRepository? repository = null;

        public WebhookConfigService(IWebhookConfigRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVWebhookConfig> GetWebhookConfigById(string orgId, string webhookConfigId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWebhookConfig()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(webhookConfigId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Webhook config ID [{webhookConfigId}] format is invalid";

                return r;
            }

            var result = await repository!.GetWebhookConfigById(webhookConfigId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Webhook config ID [{webhookConfigId}] not found for the organization [{orgId}]";

                return r;
            }

            var headersDef = result.HeadersDefinition;
            if (string.IsNullOrEmpty(headersDef))
            {
                headersDef = "{}";
            }

            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersDef);

            r.WebhookConfig = result;
            r.WebhookConfig.Headers = headers!;
            r.WebhookConfig.HeadersDefinition = "";

            return r;
        }

        public async Task<MVWebhookConfig> DeleteWebhookById(string orgId, string webhookConfigId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWebhookConfig()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(webhookConfigId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Webhook config ID [{webhookConfigId}] format is invalid";

                return r;
            }

            var currentWc = await GetWebhookConfigById(orgId, webhookConfigId);
            if (currentWc.WebhookConfig == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Webhook config ID [{webhookConfigId}] not found for the organization [{orgId}]";

                return r;
            }

            var m = await repository!.DeleteWebhookConfigById(webhookConfigId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Webhook config ID [{webhookConfigId}] not found for the organization [{orgId}]";

                return r;
            }

            r.WebhookConfig = m;
            return r;
        }

        public async Task<MVWebhookConfig> AddMerchantWebhookConfig(string orgId, string merchantId, MWebhookConfig webhookConfig)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWebhookConfig
            {
                Status = "OK",
                Description = "Success"
            };

            if (string.IsNullOrEmpty(webhookConfig.EventName))
            {
                r.Status = "EVENT_NAME_MISSING";
                r.Description = $"Event name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsEventNameExist(webhookConfig.EventName);
            if (isExist)
            {
                r.Status = "EVENT_NAME_DUPLICATE";
                r.Description = $"Event name [{webhookConfig.EventName}] already exist!!!";

                return r;
            }

            webhookConfig.Headers ??= []; //Empty array
            webhookConfig.HeadersDefinition = JsonSerializer.Serialize(webhookConfig.Headers)!;

            var result = await repository!.AddMerchantWebhookConfig(merchantId, webhookConfig);
            r.WebhookConfig = result;

            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.WebhookConfig.HeadersDefinition = "";

            return r;
        }

        public async Task<List<MWebhookConfig>> GetWebhookConfigsByMerchantId(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWebhooksByMerchantId(merchantId);

            return result;
        }

        public async Task<int> GetWebhookConfigsCountByMerchantId(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWebhookCountByMerchantId(merchantId);

            return result;
        }

        public async Task<MVWebhookConfig> UpdateWebhookConfigById(string orgId, string webhookConfigId, MWebhookConfig webhookConfig)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWebhookConfig()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(webhookConfigId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Webhook config ID [{webhookConfigId}] format is invalid";

                return r;
            }

            var eventName = webhookConfig.EventName;
            var cr = await repository!.GetWebhookConfigByEventName(eventName!);
            if ((cr != null) && (cr.WebhookId.ToString() != webhookConfigId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Webhook config name [{eventName}] already exist!!!";

                return r;
            }

            webhookConfig.Headers ??= []; //Empty array
            webhookConfig.HeadersDefinition = JsonSerializer.Serialize(webhookConfig.Headers)!;

            var result = await repository!.UpdateWebhookConfigById(webhookConfigId, webhookConfig);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Webhook config ID [{webhookConfigId}] not found for the organization [{orgId}]";

                return r;
            }

            r.WebhookConfig = result;
            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.WebhookConfig.HeadersDefinition = "";

            return r;
        }

        public async Task<MVWebhookConfig> UpdateWebhookConfigStatusById(string orgId, string webhookConfigId, bool isEnable)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWebhookConfig()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(webhookConfigId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Webhook config ID [{webhookConfigId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateWebhookConfigStatusById(webhookConfigId, isEnable);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Webhook config ID [{webhookConfigId}] not found for the organization [{orgId}]";

                return r;
            }

            r.WebhookConfig = result;
            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.WebhookConfig.HeadersDefinition = "";

            return r;
        }
    }
}
