using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PortalPageController : ControllerBase
    {
        private readonly IPortalPageService svc;

        [ExcludeFromCodeCoverage]
        public PortalPageController(IPortalPageService service)
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
        [Route("org/{id}/action/GetPortalPages")]
        public async Task<IActionResult> GetPortalPages(string id, [FromBody] VMPortalPage param)
        {
            var result = await svc.GetPortalPages(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPortalPageCount")]
        public async Task<IActionResult> GetPortalPageCount(string id, [FromBody] VMPortalPage param)
        {
            var result = await svc.GetPortalPageCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPortalPageById/{portalPageId}")]
        public async Task<IActionResult> GetPortalPageById(string id, string portalPageId)
        {
            var result = await svc.GetPortalPageById(id, portalPageId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPortalPage")]
        public async Task<IActionResult> AddPortalPage(string id, [FromBody] MPortalPage request)
        {
            var result = await svc.AddPortalPage(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePortalPageById/{portalPageId}")]
        public async Task<IActionResult> UpdatePortalPageById(string id, string portalPageId, [FromBody] MPortalPage request)
        {
            var result = await svc.UpdatePortalPageById(id, portalPageId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeletePortalPageById/{portalPageId}")]
        public async Task<IActionResult> DeletePortalPageById(string id, string portalPageId)
        {
            var result = await svc.DeletePortalPageById(id, portalPageId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnablePortalPageById/{portalPageId}")]
        public async Task<IActionResult> EnablePortalPageById(string id, string portalPageId)
        {
            var result = await svc.UpdatePortalPageStatusById(id, portalPageId, true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisablePortalPageById/{portalPageId}")]
        public async Task<IActionResult> DisablePortalPageById(string id, string portalPageId)
        {
            var result = await svc.UpdatePortalPageStatusById(id, portalPageId, false);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetDefaultPortalPageById/{portalPageId}")]
        public async Task<IActionResult> SetDefaultPortalPageById(string id, string portalPageId)
        {
            var result = await svc.SetDefaultPortalPageById(id, portalPageId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CreatePortalPageApiKey/{portalPageId}")]
        public async Task<IActionResult> CreatePortalPageApiKey(string id, string portalPageId, [FromBody] CreatePortalPageApiKeyRequest request)
        {
            var result = await svc.CreatePortalPageApiKey(id, portalPageId, request.KeyName ?? "", request.KeyDescription);
            return Ok(result);
        }
    }
}
