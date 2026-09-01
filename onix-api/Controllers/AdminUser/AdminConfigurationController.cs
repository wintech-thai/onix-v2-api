using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

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

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous] //คนที่จะมาเรียก API นี้คือ backend เอง เพื่อดูว่าจะอ่าน client ip จากไหน
        [Route("org/global/action/GetClientIpSource")]
        public async Task<IActionResult> GetClientIpSource()
        {
            var result = await svc.GetClientIpSource("global");
            result!.ResolvedIp = ResolveCurrentClientIp(result.Configuration?.ClientIpSourceConfig);

            return Ok(result);
        }

        private string? ResolveCurrentClientIp(MClientIpSourceConfig? cfg)
        {
            if (cfg == null || string.IsNullOrEmpty(cfg.SourceType) || cfg.SourceType == "Native")
            {
                var remoteAddr = HttpContext.Connection.RemoteIpAddress;
                if (remoteAddr != null && remoteAddr.IsIPv4MappedToIPv6)
                {
                    remoteAddr = remoteAddr.MapToIPv4();
                }

                return remoteAddr?.ToString();
            }

            if (string.IsNullOrEmpty(cfg.HeaderName))
            {
                return null;
            }

            var headerValue = Request.Headers[cfg.HeaderName].ToString();
            if (string.IsNullOrEmpty(headerValue))
            {
                return null;
            }

            var parts = headerValue.Split(',').Select(p => p.Trim()).ToArray();
            var index = cfg.HeaderIndex ?? 0;
            if (index < 0 || index >= parts.Length)
            {
                return null;
            }

            return parts[index];
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SetClientIpSource")]
        public async Task<IActionResult> SetClientIpSource([FromBody] MConfiguration cfg)
        {
            var result = await svc.SetClientIpSource("global", cfg);
            return Ok(result);
        }
    }
}
