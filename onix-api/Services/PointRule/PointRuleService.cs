using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using System.Threading.Tasks;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class PointRule : BaseService, IPointRuleService
    {
        private readonly IPointRuleRepository repository = null!;
        private readonly IRedisHelper _redis;

        public PointRule(
            IPointRuleRepository repo,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public async Task<MVPointRule> AddPointRule(string orgId, MPointRule pr)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(pr.RuleName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Rule name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(pr.TriggeredEvent))
            {
                r.Status = "TRUGGER_EVENT_MISSING";
                r.Description = $"Trigger event is missing!!!";

                return r;
            }

            var isRuleNameExist = await repository.IsRuleNameExist(pr.RuleName!);
            if (isRuleNameExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Rule name [{pr.RuleName}] is duplicate!!!";

                return r;
            }

            //TODO : Validate rule definitioin

            var result = await repository!.AddPointRule(pr);
            r.PointRule = result;

            return r;
        }

        public async Task<MVPointRule?> UpdatePointRuleById(string orgId, string pointRuleId, MPointRule pr)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(pr.RuleName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Rule name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(pr.TriggeredEvent))
            {
                r.Status = "TRUGGER_EVENT_MISSING";
                r.Description = $"Trigger event is missing!!!";

                return r;
            }

            //TODO : Validate rule definitioin

            var result = await repository!.UpdatePointRuleById(pointRuleId, pr);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PointRule = result;
            return r;
        }

        public async Task<MVPointRule?> GetPointRuleById(string orgId, string pointRuleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            var result = await repository!.GetPointRuleById(pointRuleId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";
            }

            r.PointRule = result;

            return r;
        }

        public async Task<MVPointRule?> DeletePointRuleById(string orgId, string pointRuleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            var m = await repository!.DeletePointRuleById(pointRuleId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PointRule = m;

            return r;
        }

        public async Task<List<MPointRule>> GetPointRules(string orgId, VMPointRule param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPointRules(param);

            return result;
        }

        public async Task<int> GetPointRulesCount(string orgId, VMPointRule param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPointRuleCount(param);

            return result;
        }

        public async Task<MVPointRule?> UpdatePointRuleStatusById(string orgId, string pointRuleId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdatePointRuleStatusById(pointRuleId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PointRule = result;
            return r;
        }
    }
}
