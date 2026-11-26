using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    // เรียกจากหน้า login-portal (public) ตอนรู้แล้วว่า portal page นี้ผูก custom theme id อะไร เอามา render - auth ด้วย api key ผ่าน Basic auth เหมือน AddPortalSession
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PortalPageThemeController : ControllerBase
    {
        private readonly IPortalPageThemeService svc;

        [ExcludeFromCodeCoverage]
        public PortalPageThemeController(IPortalPageThemeService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPortalPageThemeById/{themeId}")]
        public async Task<IActionResult> GetPortalPageThemeById(string id, string themeId)
        {
            var result = await svc.GetPortalPageThemeById(themeId);
            return Ok(result);
        }
    }
}
