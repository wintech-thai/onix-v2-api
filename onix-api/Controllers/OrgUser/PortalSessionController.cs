using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PortalSessionController : ControllerBase
    {
        private readonly IPortalSessionService svc;

        [ExcludeFromCodeCoverage]
        public PortalSessionController(IPortalSessionService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPortalSessions")]
        public async Task<IActionResult> GetPortalSessions(string id, [FromBody] VMPortalSession param)
        {
            var result = await svc.GetPortalSessions(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPortalSessionCount")]
        public async Task<IActionResult> GetPortalSessionCount(string id, [FromBody] VMPortalSession param)
        {
            var result = await svc.GetPortalSessionCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPortalSessionById/{portalSessionId}")]
        public async Task<IActionResult> GetPortalSessionById(string id, string portalSessionId)
        {
            var result = await svc.GetPortalSessionById(id, portalSessionId);
            return Ok(result);
        }
    }
}
