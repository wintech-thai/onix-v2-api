using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminRiskPolicyController : ControllerBase
    {
        private readonly IRiskPolicyService svc;

        [ExcludeFromCodeCoverage]
        public AdminRiskPolicyController(IRiskPolicyService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddRiskPolicy")]
        public async Task<IActionResult> AddRiskPolicy([FromBody] MRiskPolicy request)
        {
            var result = await svc.AddRiskPolicyV2("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteRiskPolicyById/{riskPolicyId}")]
        public async Task<IActionResult> DeleteRiskPolicyById(string riskPolicyId)
        {
            var result = await svc.DeleteRiskPolicyByIdV2("global", riskPolicyId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateRiskPolicyById/{riskPolicyId}")]
        public async Task<IActionResult> UpdateRiskPolicyById(string riskPolicyId, [FromBody] MRiskPolicy request)
        {
            var result = await svc.UpdateRiskPolicyByIdV2("global", riskPolicyId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableRiskPolicyById/{riskPolicyId}")]
        public async Task<IActionResult> EnableRiskPolicyById(string riskPolicyId)
        {
            var result = await svc.UpdateRiskPolicyStatusByIdV2("global", riskPolicyId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableRiskPolicyById/{riskPolicyId}")]
        public async Task<IActionResult> DisableRiskPolicyById(string riskPolicyId)
        {
            var result = await svc.UpdateRiskPolicyStatusByIdV2("global", riskPolicyId, "Disabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetRiskPolicyById/{riskPolicyId}")]
        public async Task<IActionResult> GetRiskPolicyById(string riskPolicyId)
        {
            var result = await svc.GetRiskPolicyByIdV2("global", riskPolicyId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetRiskPolicies")]
        public async Task<IActionResult> GetRiskPolicies([FromBody] VMRiskPolicy request)
        {
            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }

            var result = await svc.GetRiskPoliciesV2("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetRiskPolicyCount")]
        public async Task<IActionResult> GetRiskPolicyCount([FromBody] VMRiskPolicy request)
        {
            var result = await svc.GetRiskPolicyCountV2("global", request);
            return Ok(result);
        }
    }
}
