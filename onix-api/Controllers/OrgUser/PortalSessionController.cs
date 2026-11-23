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

        // org-id = OrgCustomId, tenant-id = internal OrgId guid (kept for traceability only, Organization=Tenant 1:1 here)
        // เรียกจากหน้า login-portal ตอน Mikrotik redirect ผู้ใช้มา - auth ด้วย api key ผ่าน Basic auth (ดู BasicAuthenticationRepo)
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [HttpGet]
        [Route("org/{id}/action/AddPortalSession/{tenantId}")]
        public async Task<IActionResult> AddPortalSession(string id, string tenantId)
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

            parameters.TryGetValue("code", out var code);
            var apiKeyId = HttpContext.Items["Temp-Identity-Id"]?.ToString();

            var result = await svc.AddPortalSession(id, code, apiKeyId, parameters);
            return Ok(result);
        }

        public class VerifyNativeLoginRequest
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        // org-id = OrgCustomId - เรียกจากหน้า login-portal ตอนผู้ใช้กรอก user/password - auth ด้วย api key ผ่าน Basic auth เหมือน AddPortalSession
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/VerifyNativeLogin/{portalSessionId}")]
        public async Task<IActionResult> VerifyNativeLogin(string id, string portalSessionId, [FromBody] VerifyNativeLoginRequest request)
        {
            var result = await svc.VerifyNativeLogin(id, portalSessionId, request.UserName ?? "", request.Password ?? "");
            return Ok(result);
        }
    }
}
