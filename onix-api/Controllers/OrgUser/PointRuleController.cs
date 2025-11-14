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
    public class PointRuleController : ControllerBase
    {
        private readonly IPointRuleService svc;

        [ExcludeFromCodeCoverage]
        public PointRuleController(IPointRuleService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPointRule")]
        public async Task<IActionResult> AddPointRule(string id, [FromBody] MPointRule request)
        {
            var result = await svc.AddPointRule(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePointRuleById/{pointRuleId}")]
        public async Task<IActionResult> UpdatePointRuleById(string id, string pointRuleId, [FromBody] MPointRule request)
        {
            var result = await svc.UpdatePointRuleById(id, pointRuleId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePointRuleStatusById/{pointRuleId}/{status}")]
        public async Task<IActionResult> UpdatePointRuleStatusById(string id, string pointRuleId, string status)
        {
            var result = await svc.UpdatePointRuleStatusById(id, pointRuleId, status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointRulesCount")]
        public async Task<IActionResult> GetPointRulesCount(string id, [FromBody] VMPointRule param)
        {
            var result = await svc.GetPointRulesCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointRules")]
        public async Task<IActionResult> GetPointRules(string id, [FromBody] VMPointRule param)
        {
            var result = await svc.GetPointRules(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPointRuleById/{pointRuleId}")]
        public async Task<IActionResult> GetPointRuleById(string id, string pointRuleId)
        {
            var result = await svc.GetPointRuleById(id, pointRuleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeletePointRuleById/{pointRuleId}")]
        public async Task<IActionResult> DeletePointRuleById(string id, string pointRuleId)
        {
            var result = await svc.DeletePointRuleById(id, pointRuleId);
            return Ok(result);
        }
    }
}
