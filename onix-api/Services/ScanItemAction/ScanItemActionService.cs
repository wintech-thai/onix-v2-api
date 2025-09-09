using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ScanItemActionService : BaseService, IScanItemActionService
    {
        private readonly IScanItemActionRepository? repository = null;
        private readonly RedisHelper _redis;

        public ScanItemActionService(IScanItemActionRepository repo, RedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public MScanItemAction GetScanItemActionById(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemActionById(actionId);

            return result;
        }

        public MScanItemAction GetScanItemAction(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemAction();

            return result;
        }

        public MVScanItemAction? AddScanItemAction(string orgId, MScanItemAction action)
        {
            //Allow only 1 in organization
            var param = new VMScanItemAction()
            {
                FullTextSearch = ""
            };

            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemAction();
            r.Status = "OK";
            r.Description = "Success";

            var actionCount = GetScanItemActionCount(orgId, param);
            if (actionCount > 0)
            {
                r.Status = "NOT_ALLOW_MORE_THAN_ONE";
                r.Description = $"Found more than 1 scan-item ({actionCount}) action in organization [{orgId}]";

                return r;
            }

            var result = repository!.AddScanItemAction(action);
            r.ScanItemAction = result;

            return r;
        }

        public MVScanItemAction? UpdateScanItemActionById(string orgId, string actionId, MScanItemAction action)
        {
            var r = new MVScanItemAction()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateScanItemActionById(actionId, action);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemAction ID [{actionId}] not found for the organization [{orgId}]";

                return r;
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemActionKey(orgId));

            r.ScanItemAction = result;
            return r;
        }

        public MVScanItemAction? DeleteScanItemActionById(string orgId, string actionId)
        {
            var r = new MVScanItemAction()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemAction ID [{actionId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteScanItemActionById(actionId);

            r.ScanItemAction = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemAction ID [{actionId}] not found for the organization [{orgId}]";
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemActionKey(orgId));
            return r;
        }

        public int GetScanItemActionCount(string orgId, VMScanItemAction param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemActionCount(param);

            return result;
        }
    }
}
