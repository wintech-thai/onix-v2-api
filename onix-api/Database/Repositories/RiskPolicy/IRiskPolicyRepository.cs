using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IRiskPolicyRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MRiskPolicy> AddRiskPolicyV2(MRiskPolicy riskPolicy);
        public Task<int> GetRiskPolicyCountV2(VMRiskPolicy param);
        public Task<List<MRiskPolicy>> GetRiskPoliciesV2(VMRiskPolicy param);
        public Task<MRiskPolicy?> GetRiskPolicyByIdV2(string riskPolicyId);
        public Task<MRiskPolicy?> DeleteRiskPolicyByIdV2(string riskPolicyId);
        public Task<MRiskPolicy?> UpdateRiskPolicyByIdV2(string riskPolicyId, MRiskPolicy riskPolicy);
        public Task<MRiskPolicy?> UpdateRiskPolicyStatusByIdV2(string riskPolicyId, string status);
    }
}
