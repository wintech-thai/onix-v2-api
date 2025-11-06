using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class PricingPlanItemController : ControllerBase
    {
        private readonly IPricingPlanItemService svc;

        [ExcludeFromCodeCoverage]
        public PricingPlanItemController(IPricingPlanItemService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPricingPlanItem")]
        public MVPricingPlanItem? AddPricingPlanItem(string id, [FromBody] MPricingPlanItem request)
        {
            var result = svc.AddPricingPlanItem(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeletePricingPlanItemById/{ppiId}")]
        public IActionResult DeletePricingPlanItemById(string id, string ppiId)
        {
            var result = svc.DeletePricingPlanItemById(id, ppiId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePricingPlanItemById/{ppiId}")]
        public IActionResult UpdatePricingPlanItemById(string id, string ppiId, [FromBody] MPricingPlanItem request)
        {
            var result = svc.UpdatePricingPlanItemById(id, ppiId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPricingPlanItemById/{ppiId}")]
        public MPricingPlanItem GetPricingPlanItemById(string id, string ppiId)
        {
            var result = svc.GetPricingPlanItemById(id, ppiId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetPricingPlanItems")]
        public IActionResult GetPricingPlanItems(string id, [FromBody] VMPricingPlanItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetPricingPlanItems(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetPricingPlanItemCount")]
        public IActionResult GetPricingPlanItemCount(string id, [FromBody] VMPricingPlanItem param)
        {
            var result = svc.GetPricingPlanItemCount(id, param);
            return Ok(result);
        }
    }
}
