using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class AgentService : BaseService, IAgentService
    {
        private readonly IAgentRepository? repository = null;
        private readonly IApiKeyRepository? _apiKeyRepo = null;
        private readonly IOrganizationRepository? _orgRepo = null;

        public AgentService(IAgentRepository repo, 
            IApiKeyRepository apiKeyRepo,
            IOrganizationRepository orgRepo) : base()
        {
            repository = repo;
            _apiKeyRepo = apiKeyRepo;
            _orgRepo = orgRepo;
        }

        public async Task<MVAgent> GetAgentById(string orgId, string agentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgent()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent ID [{agentId}] format is invalid";

                return r;
            }

            var result = await repository!.GetAgentById(agentId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent ID [{agentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Agent = result;

            return r;
        }

        public async Task<MVAgent> AddAgent(string orgId, MAgent agent)
        {
            repository!.SetCustomOrgId(orgId);
            _apiKeyRepo!.SetCustomOrgId(orgId);
            _orgRepo!.SetCustomOrgId(orgId);

            var r = new MVAgent()
            {
                Status = "OK",
                Description = "Success",
            };


            if (string.IsNullOrEmpty(agent.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = $"Agent code is missing!!!";

                return r;
            }

            var isExist = await repository!.IsAgentCodeExist(agent.Code);
            if (isExist)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Agent code [{agent.Code}] already exist!!!";

                return r;
            }

            //Add API key here and return key & registration link
            var apiKey = new MApiKey()
            {
                KeyName = $"Agent:{agent.Code}",
                KeyStatus = "Active",
                KeyDescription = $"API key for agent [{agent.Code}]",

                //ให้ user กำหนด Role เอาเองไปก่อน
            };

            var k = _apiKeyRepo.AddApiKey(apiKey);

            agent.ApiKeyId = k.KeyId.ToString()!;
            var result = await repository!.AddAgent(agent);
            var agentId = result.Id.ToString();

            var org = _orgRepo.GetOrganization().Result;
            var domain = ServiceUtils.GetDomain(org.OrgType!, "api");

            result.RegistrationUrl = $"https://{domain}/api/Agent/org/{orgId}/action/Heartbeat/{agentId}";
            result.ApiKey = k.ApiKey;

            r.Agent = result;

            return r;
        }

        public async Task<MVAgent> DeleteAgentById(string orgId, string agentId)
        {
            repository!.SetCustomOrgId(orgId);
            _apiKeyRepo!.SetCustomOrgId(orgId);

            var r = new MVAgent()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent ID [{agentId}] format is invalid";

                return r;
            }

            var currentAgent = await GetAgentById(orgId, agentId);
            if (currentAgent.Agent == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent ID [{agentId}] not found for the organization [{orgId}]";

                return r;
            }

            //Delete API key ด้วย
            var apiKeyId = currentAgent.Agent.ApiKeyId;
            var k = _apiKeyRepo.DeleteApiKeyById(apiKeyId);

            if (k == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"API key ID [{apiKeyId}] not found for the organization [{orgId}]";

                return r;
            }

            var m = await repository!.DeleteAgentById(agentId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent ID [{agentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Agent = m;
            return r;
        }

        public async Task<List<MAgent>> GetAgents(string orgId, VMAgent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgents(param);

            return result;
        }

        public async Task<int> GetAgentCount(string orgId, VMAgent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentCount(param);

            return result;
        }

        public async Task<MVAgent> UpdateAgentById(string orgId, string agentId, MAgent agent)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgent()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent ID [{agentId}] format is invalid";

                return r;
            }

            var code = agent.Code;
            var cr = await repository!.GetAgentByCode(code!);
            if ((cr != null) && (cr.Id.ToString() != agentId))
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Agent code [{code}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdateAgentById(agentId, agent);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent ID [{agentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Agent = result;

            return r;
        }
    }
}
