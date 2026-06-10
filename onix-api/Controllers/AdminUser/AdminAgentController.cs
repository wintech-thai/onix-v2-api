using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminAgentController : ControllerBase
    {
        private readonly IAgentService svc;
        private readonly IApiKeyService _apiKeySvc;

        [ExcludeFromCodeCoverage]
        public AdminAgentController(IAgentService service, IApiKeyService apiKeySvc)
        {
            svc = service;
            _apiKeySvc = apiKeySvc;
        }


        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetAgentEndPoints/{agentId}")]
        public async Task<IActionResult> GetAgentEndPoints(string agentId)
        {
            var mvAgent = await svc.GetAgentById("global", agentId);
            if (mvAgent.Status != "OK")
            {
                return Ok(mvAgent);
            }

            var mc = mvAgent.Agent!;

            var agentOrgId = mc.OrgId;
            var url1 = $"https://<PAYMENT-REQUEST-SERVICE>/admin-api/PaymentRequest/org/{agentOrgId}/action/Heartbeat/{agentId}";
            var url2 = $"https://<PAYMENT-REQUEST-SERVICE>/admin-api/PaymentRequest/org/{agentOrgId}/action/PaymentTxNoti/{agentId}";

            var result = new MVEndPoint()
            {
                Status = "OK",
                Description = "Success",
                AgentHeartbeatUrl = url1,
                PaymentTxNotiUrl = url2,
            };

            return Ok(result);
        }


        [HttpPost]
        [Route("org/global/action/CreateAgentApiKey/{agentId}")]
        public IActionResult CreateAgentApiKey(string agentId)
        {
            var uuid = Guid.NewGuid();

            var request = new MApiKey()
            {
                KeyType = $"Agent:{agentId}",
                KeyName = $"Agent:{uuid}",
                KeyDescription = "Auto generated key, DO NOT delete!!!",
                Roles = [ "AGENT_CONNECT" ], 
            };

            var apiKey = _apiKeySvc.AddApiKey("global", request);
            return Ok(apiKey);
        }

        [HttpGet]
        [Route("org/global/action/GetAgentApiKeys/{agentId}")]
        public IActionResult GetAgentApiKeys(string agentId)
        {
            var request = new VMApiKey()
            {
                KeyType = $"Agent:{agentId}", 
            };

            var keys = _apiKeySvc.GetApiKeys("global", request);

            return Ok(keys);
        }


        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddAgent")]
        public async Task<IActionResult> AddAgent([FromBody] MAgent request)
        {
            var result = await svc.AddAgentSimple("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteAgentById/{agentId}")]
        public async Task<IActionResult> DeleteAgentById(string agentId)
        {
            var result = await svc.DeleteAgentById("global", agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetAgentById/{agentId}")]
        public async Task<IActionResult> GetAgentById(string agentId)
        {
            var result = await svc.GetAgentById("global", agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateAgentById/{agentId}")]
        public async Task<IActionResult> UpdateAgentById(string agentId, [FromBody] MAgent request)
        {
            var result = await svc.UpdateAgentById("global", agentId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetAgents")]
        public async Task<IActionResult> GetAgents([FromBody] VMAgent request)
        {
            var result = await svc.GetAgents("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetAgentCount")]
        public async Task<IActionResult> GetAgentCount([FromBody] VMAgent request)
        {
            var result = await svc.GetAgentCount("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/Heartbeat/{agentId}")]
        public async Task<IActionResult> Heartbeat(string agentId, [FromBody] JsonElement body)
        {
            var result = await svc.AddAgentStat("global", agentId, body);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/PaymentTxNoti/{agentId}")]
        public async Task<IActionResult> PaymentTxNoti(string agentId, [FromBody] JsonElement body)
        {
            var result = await svc.AddAgentStat("global", agentId, body);
            return Ok(result);
        }
    }
}
