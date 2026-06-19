using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using Its.Onix.Api.ModelsViews;
using System.Text.RegularExpressions;
using QRCoder;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminFinancialDocController : ControllerBase
    {
        private readonly IFinancialDocService svc;

        [ExcludeFromCodeCoverage]
        public AdminFinancialDocController(IFinancialDocService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddFinancialDoc")]
        public async Task<IActionResult> AddFinancialDoc([FromBody] MFinancialDoc request)
        {
            var result = await svc.AddFinancialDoc("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateFinancialDocById/{financialDocId}")]
        public async Task<IActionResult> UpdateFinancialDocById(string financialDocId, [FromBody] MFinancialDoc request)
        {
            var result = await svc.UpdateFinancialDocById("global", financialDocId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteFinancialDocById/{financialDocId}")]
        public async Task<IActionResult> DeleteFinancialDocById(string financialDocId)
        {
            var result = await svc.DeleteFinancialDocById("global", financialDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetFinancialDocById/{financialDocId}")]
        public async Task<IActionResult> GetFinancialDocById(string financialDocId)
        {
            var result = await svc.GetFinancialDocById("global", financialDocId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetFinancialDocs")]
        public async Task<IActionResult> GetFinancialDocs([FromBody] VMFinancialDoc request)
        {
            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }

            var result = await svc.GetFinancialDocs("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetFinancialDocCount")]
        public async Task<IActionResult> GetFinancialDocCount([FromBody] VMFinancialDoc request)
        {
            var result = await svc.GetFinancialDocCount("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/CalculateRevenue")]
        public async Task<IActionResult> CalculateRevenue([FromBody] VMSummary request)
        {
            var fromDate = request.FromDate ?? DateTime.UtcNow.Date;
            var toDate = request.ToDate ?? DateTime.UtcNow;

            var result = await svc.CalculateRevenue("global", fromDate, toDate);
            return Ok(result);
        }
    }
}
