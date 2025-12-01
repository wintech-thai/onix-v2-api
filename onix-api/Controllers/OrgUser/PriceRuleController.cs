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
    public class PriceRuleController : ControllerBase
    {
        private readonly IPointRuleService svc;

        [ExcludeFromCodeCoverage]
        public PriceRuleController(IPointRuleService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPriceRule")]
        public async Task<IActionResult> AddPriceRule(string id, [FromBody] MPointRule request)
        {
            request.RuleType = "PriceRule";
            var result = await svc.AddPointRule(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePriceRuleById/{priceRuleId}")]
        public async Task<IActionResult> UpdatePriceRuleById(string id, string priceRuleId, [FromBody] MPointRule request)
        {
            request.RuleType = "PriceRule";
            var result = await svc.UpdatePointRuleById(id, priceRuleId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePriceRuleStatusByIdActive/{priceRuleId}")]
        public async Task<IActionResult> UpdatePriceRuleStatusByIdActive(string id, string priceRuleId)
        {
            var result = await svc.UpdatePointRuleStatusById(id, priceRuleId, "Active");

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePriceRuleStatusByIdDisable/{priceRuleId}")]
        public async Task<IActionResult> UpdatePriceRuleStatusByIdDisable(string id, string priceRuleId)
        {
            var result = await svc.UpdatePointRuleStatusById(id, priceRuleId, "Disable");

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPriceRulesCount")]
        public async Task<IActionResult> GetPriceRulesCount(string id, [FromBody] VMPointRule param)
        {
            param.RuleType = "PriceRule";
            var result = await svc.GetPointRulesCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPriceRules")]
        public async Task<IActionResult> GetPriceRules(string id, [FromBody] VMPointRule param)
        {
            param.RuleType = "PriceRule";
            var result = await svc.GetPointRules(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPriceRuleById/{priceRuleId}")]
        public async Task<IActionResult> GetPriceRuleById(string id, string priceRuleId)
        {
            var result = await svc.GetPointRuleById(id, priceRuleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeletePriceRuleById/{priceRuleId}")]
        public async Task<IActionResult> DeletePriceRuleById(string id, string priceRuleId)
        {
            var result = await svc.DeletePointRuleById(id, priceRuleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EvaluatePriceRuleById/{priceRuleId}")]
        public async Task<IActionResult> EvaluatePriceRuleById(string id, string priceRuleId, [FromBody] PointRuleInput param)
        {
            var result = await svc.EvaluatePointRuleById(id, priceRuleId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/TestPriceRule")]
        public async Task<IActionResult> TestPriceRule(string id, [FromBody] PointRuleInput param)
        {
            var result = await svc.TestPointRule(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EvaluatePriceRules/{triggerEvent}")]
        public async Task<IActionResult> EvaluatePriceRules(string id, string triggerEvent, [FromBody] PointRuleInput param)
        {
            var result = await svc.EvaluatePointRules(id, triggerEvent, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPriceRuleInputFields/{triggerEvent}")]
        public IActionResult GetPriceRuleInputFields(string id, string triggerEvent)
        {
            var result = svc.GetRuleInputFields(id, triggerEvent, false);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPriceRuleEvaluateInputFields/{triggerEvent}")]
        public IActionResult GetPriceRuleEvaluateInputFields(string id, string triggerEvent)
        {
            var result = svc.GetRuleInputFields(id, triggerEvent, true);
            return Ok(result);
        }
    }
}
