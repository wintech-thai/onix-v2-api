using System.Text.Json;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IAgentPolicyService
    {
        public Task<MVAgentPolicy> GetAgentPolicyById(string orgId, string agentPolicyId);
        public Task<MVAgentPolicy> AddAgentPolicy(string orgId, MAgentPolicy agentPolicy);
        public Task<MVAgentPolicy> DeleteAgentPolicyById(string orgId, string agentPolicyId);
        public Task<List<MAgentPolicy>> GetAgentPolicies(string orgId, VMAgentPolicy param);
        public Task<int> GetAgentPolicyCount(string orgId, VMAgentPolicy param);
        public Task<MVAgentPolicy> UpdateAgentPolicyById(string orgId, string agentPolicyId, MAgentPolicy agentPolicy);
    }
}
