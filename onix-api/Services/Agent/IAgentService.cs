using System.Text.Json;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;


namespace Its.Onix.Api.Services
{
    public interface IAgentService
    {
        public (string, string) GetAgentEndpoints(string agentId);
        public Task<MVAgent> GetAgentById(string orgId, string agentId);
        public Task<MVAgent> AddAgent(string orgId, MAgent agent);
        public Task<MVAgent> AddAgentSimple(string orgId, MAgent agent);
        
        public Task<MVAgent> DeleteAgentById(string orgId, string agentId);
        public Task<List<MAgent>> GetAgents(string orgId, VMAgent param);
        public Task<int> GetAgentCount(string orgId, VMAgent param);
        public Task<MVAgent> UpdateAgentById(string orgId, string agentId, MAgent agent);
        public Task<MVAgent> AddAgentStat(string orgId, string agentId, JsonElement body);

        public Task<int> GetAgentEventCount(string orgId, VMAgentEvent param);
        public Task<List<MAgentEvent>> GetAgentEvents(string orgId, VMAgentEvent param);
        public Task<MVAgentEvent> GetAgentEventById(string orgId, string agentEventId);
        public Task<MVAgentEvent> AddAgentEvent(string orgId, MAgentEvent evt);
        public Task<List<MVAgentEventTimeSeries>> GetAgentEventTimeSeries(string orgId, VMAgentEvent param);

        public Task<MVAgent> AddLineApiAgent(string orgId, MAgent agent);
        public Task<MVAgent> RestartLineApiAgentById(string orgId, string agentId);
        public Task<MVAgent> ReloadLineApiAgentById(string orgId, string agentId);
    }
}
