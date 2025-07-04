using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ApiKeyService : BaseService, IApiKeyService
    {
        private readonly IApiKeyRepository? repository = null;
        private DateTime compareDate = DateTime.Now;

        public ApiKeyService(IApiKeyRepository repo) : base()
        {
            repository = repo;
        }

        public void SetCompareDate(DateTime dtm)
        {
            //For unit testing injection
            compareDate = dtm;
        }

        public Task<MApiKey> GetApiKey(string orgId, string apiKey)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKey(apiKey);

            return result;
        }

        public MVApiKey VerifyApiKey(string orgId, string apiKey)
        {
            repository!.SetCustomOrgId(orgId);
            var m = repository!.GetApiKey(apiKey).Result;

            var status = "OK";
            var description = "SUCCESS";

            if (m == null)
            {
                status = "NOTFOUND";
                description = $"API key not found for the organization [{orgId}]";
            }
            else if ((m.KeyExpiredDate != null) && (DateTime.Compare(compareDate, (DateTime)m.KeyExpiredDate!) > 0))
            {
                status = "EXPIRED";
                description = $"API key for the organization is expire [{orgId}] since [{m.KeyExpiredDate}]";
            }

            var mv = new MVApiKey()
            {
                ApiKey = m,
                Status = status,
                Description = description,
            };

            return mv;
        }

        public MVApiKey? AddApiKey(string orgId, MApiKey apiKey)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVApiKey();
            var t = repository!.GetApiKey(apiKey.ApiKey!);
            var m = t.Result;

            if (m != null)
            {
                r.Status = "DUPLICATE";
                r.Description = "API Key is duplicate";

                return r;
            }

            var result = repository!.AddApiKey(apiKey);

            r.Status = "OK";
            r.Description = "Success";
            r.ApiKey = result;

            //Demo
            return r;
        }

        public MVApiKey? DeleteApiKeyById(string orgId, string keyId)
        {
            var r = new MVApiKey()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(keyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Key ID [{keyId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteApiKeyById(keyId);

            r.ApiKey = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Key ID [{keyId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MApiKey> GetApiKeys(string orgId, VMApiKey param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKeys(param);

            return result;
        }

        public int GetApiKeyCount(string orgId, VMApiKey param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKeyCount(param);

            return result;
        }

        public MVApiKey? UpdateApiKeyById(string orgId, string keyId, MApiKey apiKey)
        {
            var r = new MVApiKey()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateApiKeyById(keyId, apiKey);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Key ID [{keyId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ApiKey = result;
            return r;
        }

        public MApiKey GetApiKeyById(string orgId, string keyId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKeyById(keyId);

            return result.Result;
        }
    }
}
