using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PointTriggerController : ControllerBase
    {
        private readonly IPointTriggerService svc;

        [ExcludeFromCodeCoverage]
        public PointTriggerController(IPointTriggerService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPointTrigger")]
        public async Task<IActionResult> AddPointTrigger(string id, [FromBody] PointTriggerInput request)
        {
            var result = await svc.AddPointTrigger(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [AllowAnonymous] //อันนี้จะใช้ token ในการ authen แทนนะ เพื่อให้ job เป็นคนเรียกเข้ามาเป็น internal
        [HttpPost]
        [Route("org/{id}/action/AddPointTrigger/{token}")]
        public async Task<IActionResult> AddPointTrigger(string id, string token, [FromBody] PointTriggerInput request)
        {
            var result = await svc.AddPointTrigger(id, token, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointTriggersCount")]
        public async Task<IActionResult> GetPointTriggersCount(string id, [FromBody] VMPointTrigger param)
        {
            var result = await svc.GetPointTriggersCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointTriggers")]
        public async Task<IActionResult> GetPointTriggers(string id, [FromBody] VMPointTrigger param)
        {
            var result = await svc.GetPointTriggers(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPointTriggerById/{pointTriggerId}")]
        public async Task<IActionResult> GetPointTriggerById(string id, string pointTriggerId)
        {
            var result = await svc.GetPointTriggerById(id, pointTriggerId);
            return Ok(result);
        }
    }
}
