using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class RiskPolicyService : BaseService, IRiskPolicyService
    {
        private readonly IRiskPolicyRepository? repository = null;
        private readonly IRedisHelper _redis;

        public RiskPolicyService(IRiskPolicyRepository repo, IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public async Task<MVRiskPolicy> GetRiskPolicyByIdV2(string orgId, string riskPolicyId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVRiskPolicy()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(riskPolicyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Risk Policy ID [{riskPolicyId}] format is invalid";

                return r;
            }

            var result = await repository!.GetRiskPolicyByIdV2(riskPolicyId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Risk Policy ID [{riskPolicyId}] not found";

                return r;
            }

            r.RiskPolicy = result;
            return r;
        }

        public async Task<MVRiskPolicy> AddRiskPolicyV2(string orgId, MRiskPolicy riskPolicy)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVRiskPolicy()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(riskPolicy.Name))
            {
                r.Status = "NAME_MISSING";
                r.Description = "Name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(riskPolicy.Status))
            {
                riskPolicy.Status = "Active";
            }

            var result = await repository!.AddRiskPolicyV2(riskPolicy);

            r.RiskPolicy = result;
            return r;
        }

        public async Task<MVRiskPolicy> UpdateRiskPolicyByIdV2(string orgId, string riskPolicyId, MRiskPolicy riskPolicy)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVRiskPolicy()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(riskPolicyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Risk Policy ID [{riskPolicyId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(riskPolicy.Name))
            {
                r.Status = "NAME_MISSING";
                r.Description = "Name is missing!!!";

                return r;
            }

            var result = await repository!.UpdateRiskPolicyByIdV2(riskPolicyId, riskPolicy);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Risk Policy ID [{riskPolicyId}] not found";

                return r;
            }

            await _redis.DeleteAsync(CacheHelper.CreateRiskPolicyKey(orgId, riskPolicyId));

            r.RiskPolicy = result;
            return r;
        }

        public async Task<MVRiskPolicy> UpdateRiskPolicyStatusByIdV2(string orgId, string riskPolicyId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVRiskPolicy()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(riskPolicyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Risk Policy ID [{riskPolicyId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateRiskPolicyStatusByIdV2(riskPolicyId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Risk Policy ID [{riskPolicyId}] not found";

                return r;
            }

            await _redis.DeleteAsync(CacheHelper.CreateRiskPolicyKey(orgId, riskPolicyId));

            r.RiskPolicy = result;
            return r;
        }

        public async Task<MVRiskPolicy> DeleteRiskPolicyByIdV2(string orgId, string riskPolicyId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVRiskPolicy()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(riskPolicyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Risk Policy ID [{riskPolicyId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteRiskPolicyByIdV2(riskPolicyId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Risk Policy ID [{riskPolicyId}] not found";
            }
            else
            {
                await _redis.DeleteAsync(CacheHelper.CreateRiskPolicyKey(orgId, riskPolicyId));
            }

            r.RiskPolicy = m;
            return r;
        }

        public async Task<List<MRiskPolicy>> GetRiskPoliciesV2(string orgId, VMRiskPolicy param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetRiskPoliciesV2(param);

            return result;
        }

        public async Task<int> GetRiskPolicyCountV2(string orgId, VMRiskPolicy param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetRiskPolicyCountV2(param);

            return result;
        }
    }
}
