using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ISummaryService _summarySvc;

        [ExcludeFromCodeCoverage]
        public ReportController(ISummaryService summarySvc)
        {
            _summarySvc = summarySvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetMerchantDashboard")]
        public async Task<IActionResult> GetMerchantDashboard(string orgId, [FromBody] VMSummary param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await _summarySvc.GetRevenueSummary(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetMerchantAnalytics")]
        public async Task<IActionResult> GetMerchantAnalytics(string orgId, [FromBody] VMSummary param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await _summarySvc.GetRevenueSummary(orgId, param);
            return Ok(result);
        }
    }
}
