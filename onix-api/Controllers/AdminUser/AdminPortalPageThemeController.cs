using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // Theme เป็น global object ไม่ผูกกับ tenant ไหน จัดการได้จาก Admin เท่านั้น
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminPortalPageThemeController : ControllerBase
    {
        private readonly IPortalPageThemeService svc;

        [ExcludeFromCodeCoverage]
        public AdminPortalPageThemeController(IPortalPageThemeService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalPageThemes")]
        public async Task<IActionResult> GetPortalPageThemes([FromBody] VMPortalPageTheme param)
        {
            var result = await svc.GetPortalPageThemes(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalPageThemeCount")]
        public async Task<IActionResult> GetPortalPageThemeCount([FromBody] VMPortalPageTheme param)
        {
            var result = await svc.GetPortalPageThemeCount(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPortalPageThemeById/{themeId}")]
        public async Task<IActionResult> GetPortalPageThemeById(string themeId)
        {
            var result = await svc.GetPortalPageThemeById(themeId);
            return Ok(result);
        }

        //สำหรับ dropdown เลือก custom theme ตอนตั้งค่า portal page - filter ด้วย authType+language ของ portal page นั้น
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPortalPageThemesForSelection/{authType}/{language}")]
        public async Task<IActionResult> GetPortalPageThemesForSelection(string authType, string language)
        {
            var result = await svc.GetPortalPageThemesForSelection(authType, language);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddPortalPageTheme")]
        public async Task<IActionResult> AddPortalPageTheme([FromBody] MPortalPageTheme request)
        {
            var result = await svc.AddPortalPageTheme(request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdatePortalPageThemeById/{themeId}")]
        public async Task<IActionResult> UpdatePortalPageThemeById(string themeId, [FromBody] MPortalPageTheme request)
        {
            var result = await svc.UpdatePortalPageThemeById(themeId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeletePortalPageThemeById/{themeId}")]
        public async Task<IActionResult> DeletePortalPageThemeById(string themeId)
        {
            var result = await svc.DeletePortalPageThemeById(themeId);
            return Ok(result);
        }
    }
}
