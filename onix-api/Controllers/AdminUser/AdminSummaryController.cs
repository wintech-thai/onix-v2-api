using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminSummaryController  : ControllerBase
    {
        private readonly ISummaryService svc;

        [ExcludeFromCodeCoverage]
        public AdminSummaryController (ISummaryService service)
        {
            svc = service;
        }

        [HttpPost]
        [Route("org/global/action/GetMerchantSummary")]
        public async Task<IActionResult> GetMerchantSummary([FromBody] VMSummary request)
        {
            var result = await svc.GetMerchantSummary("global", request);
            return Ok(result);
        }
        
        [HttpPost]
        [Route("org/global/action/GetRevenueSummary")]
        public async Task<IActionResult> GetRevenueSummary([FromBody] VMSummary request)
        {
            var result = await svc.GetRevenueSummary("global", request);
            return Ok(result);
        }
    }
}
