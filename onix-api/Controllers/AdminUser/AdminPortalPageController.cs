using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // Admin ต้องจัดการ Portal Page ของ tenant ที่ตัวเองเลือกอยู่ แต่ไม่มี OrgUser token ของ tenant นั้น
    // เลยเรียก /api/PortalPage (OrgUser) ตรง ๆ ไม่ได้ (401) ต้องผ่าน /admin-api/.../org/global แล้วส่ง orgId จริงมาทาง route param แทน
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminPortalPageController : ControllerBase
    {
        private readonly IPortalPageService svc;

        [ExcludeFromCodeCoverage]
        public AdminPortalPageController(IPortalPageService service)
        {
            svc = service;
        }

        public class CreatePortalPageApiKeyRequest
        {
            public string? KeyName { get; set; }
            public string? KeyDescription { get; set; }
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalPages/{orgId}")]
        public async Task<IActionResult> GetPortalPages(string orgId, [FromBody] VMPortalPage param)
        {
            var result = await svc.GetPortalPages(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalPageCount/{orgId}")]
        public async Task<IActionResult> GetPortalPageCount(string orgId, [FromBody] VMPortalPage param)
        {
            var result = await svc.GetPortalPageCount(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPortalPageById/{orgId}/{portalPageId}")]
        public async Task<IActionResult> GetPortalPageById(string orgId, string portalPageId)
        {
            var result = await svc.GetPortalPageById(orgId, portalPageId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddPortalPage/{orgId}")]
        public async Task<IActionResult> AddPortalPage(string orgId, [FromBody] MPortalPage request)
        {
            var result = await svc.AddPortalPage(orgId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdatePortalPageById/{orgId}/{portalPageId}")]
        public async Task<IActionResult> UpdatePortalPageById(string orgId, string portalPageId, [FromBody] MPortalPage request)
        {
            var result = await svc.UpdatePortalPageById(orgId, portalPageId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeletePortalPageById/{orgId}/{portalPageId}")]
        public async Task<IActionResult> DeletePortalPageById(string orgId, string portalPageId)
        {
            var result = await svc.DeletePortalPageById(orgId, portalPageId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnablePortalPageById/{orgId}/{portalPageId}")]
        public async Task<IActionResult> EnablePortalPageById(string orgId, string portalPageId)
        {
            var result = await svc.UpdatePortalPageStatusById(orgId, portalPageId, true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisablePortalPageById/{orgId}/{portalPageId}")]
        public async Task<IActionResult> DisablePortalPageById(string orgId, string portalPageId)
        {
            var result = await svc.UpdatePortalPageStatusById(orgId, portalPageId, false);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SetDefaultPortalPageById/{orgId}/{portalPageId}")]
        public async Task<IActionResult> SetDefaultPortalPageById(string orgId, string portalPageId)
        {
            var result = await svc.SetDefaultPortalPageById(orgId, portalPageId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/CreatePortalPageApiKey/{orgId}/{portalPageId}")]
        public async Task<IActionResult> CreatePortalPageApiKey(string orgId, string portalPageId, [FromBody] CreatePortalPageApiKeyRequest request)
        {
            var result = await svc.CreatePortalPageApiKey(orgId, portalPageId, request.KeyName ?? "", request.KeyDescription);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPortalPageApiKey/{orgId}/{portalPageId}")]
        public async Task<IActionResult> GetPortalPageApiKey(string orgId, string portalPageId, [FromBody] CreatePortalPageApiKeyRequest request)
        {
            var result = await svc.GetPortalPageApiKey(orgId, portalPageId, request.KeyName ?? "", request.KeyDescription);
            return Ok(result);
        }
    }
}
