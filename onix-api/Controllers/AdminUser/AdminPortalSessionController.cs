using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminPortalSessionController : ControllerBase
    {
        private readonly IPortalSessionService svc;

        [ExcludeFromCodeCoverage]
        public AdminPortalSessionController(IPortalSessionService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalSessions")]
        public async Task<IActionResult> GetPortalSessions([FromBody] VMPortalSession param)
        {
            var result = await svc.GetPortalSessions("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalSessionCount")]
        public async Task<IActionResult> GetPortalSessionCount([FromBody] VMPortalSession param)
        {
            var result = await svc.GetPortalSessionCount("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPortalSessionById/{portalSessionId}")]
        public async Task<IActionResult> GetPortalSessionById(string portalSessionId)
        {
            var result = await svc.GetPortalSessionById("global", portalSessionId);
            return Ok(result);
        }
    }
}
