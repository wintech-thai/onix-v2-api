using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminConfigurationController : ControllerBase
    {
        private readonly IConfigurationService svc;

        [ExcludeFromCodeCoverage]
        public AdminConfigurationController(IConfigurationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableConfigById/{configId}")]
        public async Task<IActionResult> EnableConfigById(string configId)
        {
            var result = await svc.SetConfigStatusById("global", configId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableConfigById/{configId}")]
        public async Task<IActionResult> DisableConfigById(string configId)
        {
            var result = await svc.SetConfigStatusById("global", configId, "Disabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous] //ยอมให้ทุกคนเข้าถึงได้ เพราะต้องใช้ในการแสดงผลหน้าแรก
        [Route("org/global/action/GetBrandConfig")]
        public async Task<IActionResult> GetBrandConfig()
        {
            var result = await svc.GetBrandConfig("global", true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetBrandLogoUploadPresignedUrl")]
        public async Task<IActionResult> GetBrandLogoUploadPresignedUrl([FromBody] VMUploadDocument request)
        {
            var result = await svc.GetBrandLogoUploadPresignedUrl("global", request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SetBrandConfig")]
        public async Task<IActionResult> SetBrandConfig([FromBody] MConfiguration cfg)
        {
            var result = await svc.SetBrandConfig("global", cfg);
            return Ok(result);
        }

        // scope: "Backend" (the Admin/Merchant Next.js apps) or "Api" (onix-api itself) —
        // each side can be configured/tested independently.
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous] //คนที่จะมาเรียก API นี้คือ backend เอง เพื่อดูว่าจะอ่าน client ip จากไหน
        [Route("org/global/action/GetClientIpSource/{scope}")]
        public async Task<IActionResult> GetClientIpSource(string scope)
        {
            var result = await svc.GetClientIpSource("global", scope);
            var (resolved, rawHeaderValue) = ServiceUtils.ResolveClientIpDetailed(Request, result?.Configuration?.ClientIpSourceConfig);
            result!.ResolvedIp = string.IsNullOrEmpty(resolved) ? null : resolved;
            result.RawHeaderValue = rawHeaderValue;

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SetClientIpSource/{scope}")]
        public async Task<IActionResult> SetClientIpSource(string scope, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetClientIpSource("global", scope, cfg);
            return Ok(result);
        }
    }
}
