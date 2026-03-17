using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IAgentService
    {
        public Task<MVAgent> GetAgentById(string orgId, string agentId);
        public Task<MVAgent> AddAgent(string orgId, MAgent agent);
        public Task<MVAgent> DeleteAgentById(string orgId, string agentId);
        public Task<List<MAgent>> GetAgents(string orgId, VMAgent param);
        public Task<int> GetAgentCount(string orgId, VMAgent param);
        public Task<MVAgent> UpdateAgentById(string orgId, string agentId, MAgent agent);
    }
}
