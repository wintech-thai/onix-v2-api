using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class AgentService : BaseService, IAgentService
    {
        private readonly IAgentRepository? repository = null;
        private readonly IApiKeyRepository? _apiKeyRepo = null;
        private readonly IOrganizationRepository? _orgRepo = null;
        private readonly IRedisHelper _redis;

        public AgentService(IAgentRepository repo, 
            IApiKeyRepository apiKeyRepo,
            IRedisHelper redis,
            IOrganizationRepository orgRepo) : base()
        {
            repository = repo;
            _apiKeyRepo = apiKeyRepo;
            _orgRepo = orgRepo;
            _redis = redis;
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
                ApiKey = Guid.NewGuid().ToString(),

                //ให้ user กำหนด Role เอาเองไปก่อน
                //TODO : เพิ่ม default role สำหรับ agent
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

        public async Task<MVAgent> AddAgentSimple(string orgId, MAgent agent)
        {
            repository!.SetCustomOrgId(orgId);

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

            agent.ApiKeyId = "";
            var result = await repository!.AddAgent(agent);
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
            if (!string.IsNullOrEmpty(apiKeyId) && (apiKeyId != "NOTUSED"))
            {
                var k = _apiKeyRepo.DeleteApiKeyById(apiKeyId);

                if (k == null)
                {
                    r.Status = "NOTFOUND";
                    r.Description = $"API key ID [{apiKeyId}] not found for the organization [{orgId}]";

                    return r;
                }
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

        private async void PublishMessage(JsonElement agentStat)
        {
            var stream = CacheHelper.CreateAgentStatStreamKey();

            // แปลง JsonElement → Dictionary
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(agentStat.GetRawText()) ?? new Dictionary<string, object>();

            // เพิ่ม field ใหม่
            dict["LogType"] = "AgentStat";
            
            var message = JsonSerializer.Serialize(dict);

            _ = await _redis.PublishMessageAsync(stream!, message);
        }

        public async Task<MVAgent> AddAgentStat(string orgId, string agentId, JsonElement agentStat)
        {
            repository!.SetCustomOrgId(orgId);

            var cacheKey = CacheHelper.CreateAgentKey(orgId);
            var agentObj = await _redis.GetObjectAsync<MVAgent>($"{cacheKey}:{agentId}");
            if (agentObj == null)
            {

                var mvAgent = await GetAgentById(orgId, agentId);
                _ = _redis.SetObjectAsync(cacheKey, mvAgent, TimeSpan.FromMinutes(10));

                agentObj = mvAgent;
            }

            if (agentObj.Status != "OK")
            {
                return agentObj;
            }

            PublishMessage(agentStat);

            return agentObj;
        }

        //AgentEvent
        public async Task<MVAgentEvent> AddAgentEvent(string orgId, MAgentEvent evt)
        {
            var r = new MVAgentEvent()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.AddAgentEvent(evt);

            r.AgentEvent = result;

            return r;
        }

        public async Task<List<MAgentEvent>> GetAgentEvents(string orgId, VMAgentEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentEvents(param);

            result.ForEach( p => p.RawData = "");

            return result;
        }

        public async Task<int> GetAgentEventCount(string orgId, VMAgentEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentEventCount(param);

            return result;
        }

        public async Task<MVAgentEvent> GetAgentEventById(string orgId, string agentEventId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgentEvent()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentEventId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent event ID [{agentEventId}] format is invalid";

                return r;
            }

            var result = await repository!.GetAgentEventById(agentEventId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent event ID [{agentEventId}] not found for the organization [{orgId}]";

                return r;
            }

            var jsonStr = result.RawData;
            if (string.IsNullOrEmpty(jsonStr))
            {
                jsonStr = "{}";
            }

            result.RawDataObj = JsonDocument.Parse(jsonStr).RootElement.Clone();;

            r.AgentEvent = result;
            r.AgentEvent.RawData = "";

            return r;
        }
    }
}
