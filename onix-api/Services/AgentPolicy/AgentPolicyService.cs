using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class AgentPolicyService : BaseService, IAgentPolicyService
    {
        private readonly IAgentPolicyRepository? repository = null;

        public AgentPolicyService(IAgentPolicyRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVAgentPolicy> GetAgentPolicyById(string orgId, string agentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgentPolicy()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent Policy ID [{agentId}] format is invalid";

                return r;
            }

            var result = await repository!.GetAgentPolicyById(agentId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent Policy ID [{agentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AgentPolicy = result;

            return r;
        }

        public async Task<MVAgentPolicy> AddAgentPolicy(string orgId, MAgentPolicy agentPolicy)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgentPolicy()
            {
                Status = "OK",
                Description = "Success",
            };


            if (string.IsNullOrEmpty(agentPolicy.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = $"Agent Policy code is missing!!!";

                return r;
            }

            var isExist = await repository!.IsAgentPolicyCodeExist(agentPolicy.Code);
            if (isExist)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Agent Policy code [{agentPolicy.Code}] already exist!!!";

                return r;
            }

            var result = await repository!.AddAgentPolicy(agentPolicy);

            r.AgentPolicy = result;

            return r;
        }

        public async Task<MVAgentPolicy> DeleteAgentPolicyById(string orgId, string agentPolicyId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgentPolicy()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentPolicyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent Policy ID [{agentPolicyId}] format is invalid";

                return r;
            }

            var currentAgentPolicy = await GetAgentPolicyById(orgId, agentPolicyId);
            if (currentAgentPolicy.AgentPolicy == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent Policy ID [{agentPolicyId}] not found for the organization [{orgId}]";

                return r;
            }

            var m = await repository!.DeleteAgentPolicyById(agentPolicyId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent Policy ID [{agentPolicyId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AgentPolicy = m;
            return r;
        }

        public async Task<List<MAgentPolicy>> GetAgentPolicies(string orgId, VMAgentPolicy param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentPolicies(param);

            return result;
        }

        public async Task<int> GetAgentPolicyCount(string orgId, VMAgentPolicy param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAgentPolicyCount(param);

            return result;
        }

        public async Task<MVAgentPolicy> UpdateAgentPolicyById(string orgId, string agentPolicyId, MAgentPolicy agentPolicy)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAgentPolicy()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(agentPolicyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Agent Policy ID [{agentPolicyId}] format is invalid";

                return r;
            }

            var code = agentPolicy.Code;
            var cr = await repository!.GetAgentPolicyByCode(code!);
            if ((cr != null) && (cr.Id.ToString() != agentPolicyId))
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Agent Policy code [{code}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdateAgentPolicyById(agentPolicyId, agentPolicy);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Agent Policy ID [{agentPolicyId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AgentPolicy = result;

            return r;
        }
    }
}
