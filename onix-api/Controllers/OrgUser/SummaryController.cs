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
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryService _summarySvc;
        private readonly IMerchantService _merchantSvc;

        [ExcludeFromCodeCoverage]
        public SummaryController(ISummaryService summarySvc, IMerchantService merchantSvc)
        {
            _summarySvc = summarySvc;
            _merchantSvc = merchantSvc;
        }

        private async Task<string?> GetCurrentMerchantId(string orgId)
        {
            if (string.IsNullOrEmpty(orgId))
            {
                return null;
            }

            var param = new VMMerchant { Limit = 1, Offset = 0 };
            var merchants = await _merchantSvc.GetMerchants(orgId, param);

            return merchants.FirstOrDefault(m => m.OrgId == orgId)?.Id?.ToString();
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetMerchantSummary")]
        public async Task<IActionResult> GetMerchantSummary(string orgId, [FromBody] VMSummary param)
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
        [Route("org/{orgId}/action/GetRevenueSummary")]
        public async Task<IActionResult> GetRevenueSummary(string orgId, [FromBody] VMSummary param)
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
        [Route("org/{orgId}/action/GetDailySummary")]
        public async Task<IActionResult> GetDailySummary(string orgId, [FromBody] VMSummary param)
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
