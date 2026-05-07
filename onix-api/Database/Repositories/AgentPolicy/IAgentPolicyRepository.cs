using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAgentPolicyRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsAgentPolicyCodeExist(string code);
        public Task<MAgentPolicy?> GetAgentPolicyByCode(string code);

        public Task<List<MAgentPolicy>> GetAgentPolicies(VMAgentPolicy param);
        public Task<int> GetAgentPolicyCount(VMAgentPolicy param);
        public Task<MAgentPolicy?> GetAgentPolicyById(string agentId);
        public Task<MAgentPolicy> AddAgentPolicy(MAgentPolicy agent);
        public Task<MAgentPolicy?> DeleteAgentPolicyById(string agentId);
        public Task<MAgentPolicy?> UpdateAgentPolicyById(string agentId, MAgentPolicy agent);
    }
}
