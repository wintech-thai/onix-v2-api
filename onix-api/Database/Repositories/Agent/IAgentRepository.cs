using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAgentRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsAgentCodeExist(string code);
        public Task<MAgent?> GetAgentByCode(string code);

        public Task<List<MAgent>> GetAgents(VMAgent param);
        public Task<int> GetAgentCount(VMAgent param);
        public Task<MAgent?> GetAgentById(string agentId);
        public Task<MAgent> AddAgent(MAgent agent);
        public Task<MAgent?> DeleteAgentById(string agentId);
        public Task<MAgent?> UpdateAgentById(string agentId, MAgent agent);
    }
}
