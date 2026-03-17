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
    public class AgentController : ControllerBase
    {
        private readonly IAgentService svc;

        [ExcludeFromCodeCoverage]
        public AgentController(IAgentService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddAgent")]
        public async Task<IActionResult> AddAgent(string id, [FromBody] MAgent request)
        {
            var result = await svc.AddAgent(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteAgentById/{agentId}")]
        public async Task<IActionResult> DeleteAgentById(string id, string agentId)
        {
            var result = await svc.DeleteAgentById(id, agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAgentById/{agentId}")]
        public async Task<IActionResult> GetAgentById(string id, string agentId)
        {
            var result = await svc.GetAgentById(id, agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateAgentById/{agentId}")]
        public async Task<IActionResult> UpdateAgentById(string id, string agentId, [FromBody] MAgent request)
        {
            var result = await svc.UpdateAgentById(id, agentId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgents")]
        public async Task<IActionResult> GetAgents(string id, [FromBody] VMAgent request)
        {
            var result = await svc.GetAgents(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentCount")]
        public async Task<IActionResult> GetAgentCount(string id, [FromBody] VMAgent request)
        {
            var result = await svc.GetAgentCount(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/Heartbeat/{agentId}")]
        public async Task<IActionResult> Heartbeat(string id, string agentId)
        {
            //TODO : เปลี่ยนตรงนี้อีกทีนะ
            var result = await svc.GetAgentById(id, agentId);
            return Ok(result);
        }
        
    }
}
