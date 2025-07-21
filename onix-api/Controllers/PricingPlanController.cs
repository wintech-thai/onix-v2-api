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
    public class PricingPlanController : ControllerBase
    {
        private readonly IPricingPlanService svc;

        [ExcludeFromCodeCoverage]
        public PricingPlanController(IPricingPlanService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPricingPlan")]
        public MVPricingPlan? AddPricingPlan(string id, [FromBody] MPricingPlan request)
        {
            var result = svc.AddPricingPlan(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeletePricingPlanById/{ppId}")]
        public IActionResult DeletePricingPlanById(string id, string ppId)
        {
            var result = svc.DeletePricingPlanById(id, ppId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeletePricingPlanCascadeById/{ppId}")]
        public IActionResult DeletePricingPlanCascadeById(string id, string ppId)
        {
            //TODO : Change the implementiation here...
            var result = svc.DeletePricingPlanById(id, ppId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePricingPlanById/{ppId}")]
        public IActionResult UpdatePricingPlanById(string id, string ppId, [FromBody] MPricingPlan request)
        {
            var result = svc.UpdatePricingPlanById(id, ppId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPricingPlanById/{ppId}")]
        public MPricingPlan GetPricingPlanById(string id, string ppId)
        {
            var result = svc.GetPricingPlanById(id, ppId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetPricingPlans")]
        public IActionResult GetPricingPlans(string id, [FromBody] VMPricingPlan param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetPricingPlans(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetPricingPlanCount")]
        public IActionResult GetPricingPlanCount(string id, [FromBody] VMPricingPlan param)
        {
            var result = svc.GetPricingPlanCount(id, param);
            return Ok(result);
        }
    }
}
