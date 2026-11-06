using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // ข้อ 5 : Admin ดู/ค้นหา/approve/reject PurchaseCash ข้าม org ทุก merchant
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminPurchaseCashController : ControllerBase
    {
        private readonly IAccountDocService svc;

        [ExcludeFromCodeCoverage]
        public AdminPurchaseCashController(IAccountDocService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPurchaseCashes")]
        public async Task<IActionResult> GetPurchaseCashes([FromBody] VMAccountDoc request)
        {
            request.DocumentType = "PurchaseCash";
            var result = await svc.GetAccountDocsAdmin(request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPurchaseCashCount")]
        public async Task<IActionResult> GetPurchaseCashCount([FromBody] VMAccountDoc request)
        {
            request.DocumentType = "PurchaseCash";
            var result = await svc.GetAccountDocCountAdmin(request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPurchaseCashById/{accountDocId}")]
        public async Task<IActionResult> GetPurchaseCashById(string accountDocId)
        {
            var result = await svc.GetAccountDocByIdAdmin(accountDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/ApprovePurchaseCashById/{accountDocId}")]
        public async Task<IActionResult> ApprovePurchaseCashById(string accountDocId)
        {
            var result = await svc.ApproveAccountDocByIdAdmin(accountDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/RejectPurchaseCashById/{accountDocId}")]
        public async Task<IActionResult> RejectPurchaseCashById(string accountDocId, [FromBody] VMRejectAccountDoc request)
        {
            var result = await svc.RejectAccountDocByIdAdmin(accountDocId, request.Reason ?? "");
            return Ok(result);
        }
    }
}
