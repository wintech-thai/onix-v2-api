using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ScanItemTemplateService : BaseService, IScanItemTemplateService
    {
        private readonly IScanItemTemplateRepository? repository = null;
        private readonly RedisHelper _redis;

        public ScanItemTemplateService(IScanItemTemplateRepository repo, RedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public MScanItemTemplate GetScanItemTemplateById(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemTemplateById(actionId);

            return result;
        }

        public MScanItemTemplate GetScanItemTemplate(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemTemplate();

            return result;
        }

        public MVScanItemTemplate? AddScanItemTemplate(string orgId, MScanItemTemplate action)
        {
            //Allow only 1 in organization
            var param = new VMScanItemTemplate()
            {
                FullTextSearch = ""
            };

            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemTemplate();
            r.Status = "OK";
            r.Description = "Success";

            var actionCount = GetScanItemTemplateCount(orgId, param);
            if (actionCount > 0)
            {
                r.Status = "NOT_ALLOW_MORE_THAN_ONE";
                r.Description = $"Found more than 1 scan-item ({actionCount}) template in organization [{orgId}]";

                return r;
            }

            var result = repository!.AddScanItemTemplate(action);
            r.ScanItemTemplate = result;

            return r;
        }

        public MVScanItemTemplate? UpdateScanItemTemplateById(string orgId, string actionId, MScanItemTemplate action)
        {
            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateScanItemTemplateById(actionId, action);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemTemplate ID [{actionId}] not found for the organization [{orgId}]";

                return r;
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemTemplateKey(orgId));

            r.ScanItemTemplate = result;
            return r;
        }

        public MVScanItemTemplate? DeleteScanItemTemplateById(string orgId, string actionId)
        {
            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemTemplate ID [{actionId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteScanItemTemplateById(actionId);

            r.ScanItemTemplate = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemTemplate ID [{actionId}] not found for the organization [{orgId}]";
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemTemplateKey(orgId));
            return r;
        }

        public int GetScanItemTemplateCount(string orgId, VMScanItemTemplate param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemTemplateCount(param);

            return result;
        }
    }
}
