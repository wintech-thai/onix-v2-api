using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class AgentPolicyController : ControllerBase
    {
        private readonly IAgentPolicyService svc;

        [ExcludeFromCodeCoverage]
        public AgentPolicyController(IAgentPolicyService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddAgentPolicy")]
        public async Task<IActionResult> AddAgentPolicy(string id, [FromBody] MAgentPolicy request)
        {
            var result = await svc.AddAgentPolicy(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteAgentPolicyById/{agentPolicyId}")]
        public async Task<IActionResult> DeleteAgentPolicyById(string id, string agentPolicyId)
        {
            var result = await svc.DeleteAgentPolicyById(id, agentPolicyId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAgentPolicyById/{agentPolicyId}")]
        public async Task<IActionResult> GetAgentPolicyById(string id, string agentPolicyId)
        {
            var result = await svc.GetAgentPolicyById(id, agentPolicyId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateAgentPolicyById/{agentPolicyId}")]
        public async Task<IActionResult> UpdateAgentPolicyById(string id, string agentPolicyId, [FromBody] MAgentPolicy request)
        {
            var result = await svc.UpdateAgentPolicyById(id, agentPolicyId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentPolicies")]
        public async Task<IActionResult> GetAgentPolicies(string id, [FromBody] VMAgentPolicy request)
        {
            var result = await svc.GetAgentPolicies(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentPolicyCount")]
        public async Task<IActionResult> GetAgentPolicyCount(string id, [FromBody] VMAgentPolicy request)
        {
            var result = await svc.GetAgentPolicyCount(id, request);
            return Ok(result);
        }
    }
}
