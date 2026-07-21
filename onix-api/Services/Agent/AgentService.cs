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
        private readonly IJobService _jobSvc;

        public AgentService(IAgentRepository repo, 
            IApiKeyRepository apiKeyRepo,
            IRedisHelper redis,
            IJobService jobService,
            IOrganizationRepository orgRepo) : base()
        {
            repository = repo;
            _apiKeyRepo = apiKeyRepo;
            _orgRepo = orgRepo;
            _redis = redis;
            _jobSvc = jobService;
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

            var jsonStr = result.BankAccountsSelected;
            if (string.IsNullOrEmpty(jsonStr))
            {
                jsonStr = "[]";
            }
            var arr = JsonSerializer.Deserialize<List<MBankAccount>>(jsonStr);
            if (arr != null)
            {
                result.BankAccountsSelectedObj = arr;
            }
            else
            {
                result.BankAccountsSelectedObj = [];
            }


            var jsonStrCfg = result.AgentConfig;
            if (string.IsNullOrEmpty(jsonStrCfg))
            {
                jsonStrCfg = "{}";
            }
            var configObj = JsonSerializer.Deserialize<MAgentConfig>(jsonStrCfg);
            if (arr != null)
            {
                result.AgentConfigObj = configObj;
            }
            else
            {
                result.AgentConfigObj = null;
            }


            r.Agent = result;
            r.Agent.BankAccountsSelected = "";
            r.Agent.AgentConfig = "";

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

            //ทำ serialize ก่อน
            agent.BankAccountsSelected = SerializeBankAccountSelected(agent);

            agent.ApiKeyId = "";
            agent.AgentType = "Android App";
            var result = await repository!.AddAgent(agent);

            r.Agent = result;
            r.Agent.BankAccountsSelected = "";

            return r;
        }

        public (string, string) GetAgentEndpoints(string agentId)
        {
            var url1 = $"https://<PAYMENT-REQUEST-SERVICE>/admin-api/AdminAgent/org/global/action/NotifyHeartbeat/{agentId}";
            var url2 = $"https://<PAYMENT-REQUEST-SERVICE>/admin-api/AdminAgent/org/global/action/NotifyLineMessage/{agentId}";

            return (url1, url2);
        }

        private async Task AddJob(string orgId, string jobType, MAgent agent)
        {
            var agentType = agent.AgentType;
            if (agentType != "Line Api")
            {
                return;
            }

            MAgentConfig? agentConfigObj = null;
            if (!string.IsNullOrEmpty(agent.AgentConfig))
            {
                agentConfigObj = JsonSerializer.Deserialize<MAgentConfig>(agent.AgentConfig);
            }

            var (url1, url2) = GetAgentEndpoints(agent?.Id.ToString()!);

            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "AgentService.AddJob()",
                Type = jobType,
                Status = "Pending",
                Tags = jobType,
                RefId = agent!.Id.ToString(),

                Parameters =
                [
                    new NameValue { Name = "ORG_ID", Value = orgId },
                    new NameValue { Name = "AGENT_ID", Value = agent?.Id.ToString() },
                    new NameValue { Name = "AGENT_CODE", Value = agent?.Code },
                    new NameValue { Name = "LINE_USERNAME", Value = agentConfigObj?.UserName },
                    new NameValue { Name = "ENDPOINT_API_KEY", Value = agentConfigObj?.ApiKey },
                    new NameValue { Name = "HEARTBEAT_ENDPOINT", Value = url1 },
                    new NameValue { Name = "NOTIFICATION_ENDPOINT", Value = url2 },
                    new NameValue { Name = "AGENT_IMAGE_TAG", Value = agentConfigObj?.AgentImageTag },
                    new NameValue { Name = "AGENT_IMAGE_REPO", Value = agentConfigObj?.AgentImageRepo },
                ]
            };

            var result = _jobSvc!.AddJob(orgId, job, false); 
            var _ = result?.Job!;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"JobSubmitted:{environment}:{jobType}";
            var message = JsonSerializer.Serialize(job);

            await _redis.PublishMessageAsync(stream!, message);

            return;
        }

        public async Task<MVAgent> AddLineApiAgent(string orgId, MAgent agent)
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

            //ทำ serialize ก่อน
            agent.BankAccountsSelected = SerializeBankAccountSelected(agent);
            agent.AgentConfig = SerializeAgentConfig(agent);

            agent.ApiKeyId = "";
            agent.AgentType = "Line Api";
            var result = await repository!.AddAgent(agent);


            //สร้าง job ไปยัง Redis
            await AddJob(orgId, "Agent.Create", result);

            r.Agent = result;
            r.Agent.BankAccountsSelected = "";
            r.Agent.AgentConfig = "";

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

            //สร้าง job ไปยัง Redis
            await AddJob(orgId, "Agent.Delete", m);


            r.Agent = m;
            return r;
        }

        public async Task<List<MAgent>> GetAgents(string orgId, VMAgent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgents(param);

            result.ForEach( p => p.BankAccountsSelected = "");

            return result;
        }

        public async Task<int> GetAgentCount(string orgId, VMAgent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentCount(param);

            return result;
        }

        private string SerializeBankAccountSelected(MAgent agent)
        {
            var bankAccounts = agent.BankAccountsSelectedObj;
            if (bankAccounts == null)
            {
                return "[]";
            }

            var jsonString = JsonSerializer.Serialize(bankAccounts);
            return jsonString;
        }


        private string SerializeAgentConfig(MAgent agent)
        {
            var cfg = agent.AgentConfigObj;
            if (cfg == null)
            {
                return "{}";
            }

            var jsonString = JsonSerializer.Serialize(cfg);
            return jsonString;
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

            //ทำ serialize ก่อน
            agent.BankAccountsSelected = SerializeBankAccountSelected(agent);
            agent.AgentConfig = SerializeAgentConfig(agent);

            var result = await repository!.UpdateAgentById(agentId, agent);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent ID [{agentId}] not found for the organization [{orgId}]";

                return r;
            }

            //สร้าง job ไปยัง Redis
            await AddJob(orgId, "Agent.Update", result);

            r.Agent = result;
            r.Agent.BankAccountsSelected = "";
            r.Agent.AgentConfig = "";

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
            r.AgentEvent.RawData = "";

            return r;
        }

        public async Task<List<MAgentEvent>> GetAgentEvents(string orgId, VMAgentEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentEvents(param);

            result.ForEach(p => {
                if (!string.IsNullOrEmpty(p.RawData))
                {
                    try { p.RawDataObj = JsonSerializer.Deserialize<Dictionary<string, object>>(p.RawData); }
                    catch { p.RawDataObj = []; }
                }
                p.RawData = "";
            });

            return result;
        }

        public async Task<int> GetAgentEventCount(string orgId, VMAgentEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentEventCount(param);

            return result;
        }

        public async Task<List<MVAgentEventTimeSeries>> GetAgentEventTimeSeries(string orgId, VMAgentEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentEventTimeSeries(param);
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

            result.RawDataObj = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);

            r.AgentEvent = result;
            r.AgentEvent.RawData = "";

            return r;
        }
    }
}
