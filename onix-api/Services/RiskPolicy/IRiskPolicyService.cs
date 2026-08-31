using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IRiskPolicyService
    {
        public Task<MVRiskPolicy> GetRiskPolicyByIdV2(string orgId, string riskPolicyId);
        public Task<MVRiskPolicy> AddRiskPolicyV2(string orgId, MRiskPolicy riskPolicy);
        public Task<MVRiskPolicy> UpdateRiskPolicyByIdV2(string orgId, string riskPolicyId, MRiskPolicy riskPolicy);
        public Task<MVRiskPolicy> UpdateRiskPolicyStatusByIdV2(string orgId, string riskPolicyId, string status);
        public Task<MVRiskPolicy> DeleteRiskPolicyByIdV2(string orgId, string riskPolicyId);
        public Task<List<MRiskPolicy>> GetRiskPoliciesV2(string orgId, VMRiskPolicy param);
        public Task<int> GetRiskPolicyCountV2(string orgId, VMRiskPolicy param);
    }
}
