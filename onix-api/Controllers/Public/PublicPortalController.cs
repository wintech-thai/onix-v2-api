using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    // /public-api/* skips OrgId resolution + auth middleware; Mikrotik calls this with no token, key param is the auth instead
    [AllowAnonymous]
    [ApiController]
    [Route("/public-api/[controller]")]
    public class PublicPortalController : ControllerBase
    {
        private readonly IPortalSessionService _portalSessionSvc;

        [ExcludeFromCodeCoverage]
        public PublicPortalController(IPortalSessionService portalSessionService)
        {
            _portalSessionSvc = portalSessionService;
        }

        // org-id = OrgCustomId, tenant-id = internal OrgId guid (kept for traceability only, Organization=Tenant 1:1 here)
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [HttpGet]
        [Route("action/AddPortalSession/{orgId}/{tenantId}")]
        public async Task<IActionResult> AddPortalSession(string orgId, string tenantId)
        {
            var parameters = new Dictionary<string, string>();

            foreach (var q in Request.Query)
            {
                parameters[q.Key] = q.Value.ToString();
            }

            if (Request.HasFormContentType)
            {
                foreach (var f in Request.Form)
                {
                    parameters[f.Key] = f.Value.ToString();
                }
            }

            parameters["__tenantId"] = tenantId; //เก็บไว้เพื่อ traceability

            parameters.TryGetValue("key", out var apiKeyValue);
            parameters.TryGetValue("code", out var code);

            var result = await _portalSessionSvc.AddPortalSessionPublic(orgId, code, apiKeyValue, parameters);
            return Ok(result);
        }
    }
}
