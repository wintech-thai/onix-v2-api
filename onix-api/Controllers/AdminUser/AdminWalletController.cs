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
    [Route("/admin-api/[controller]")]
    public class AdminWalletController : ControllerBase
    {
        private readonly IPointService svc;
        private readonly IMerchantService _merchantSvc;
        private readonly IBankAccountService _bankAccountSvc;

        [ExcludeFromCodeCoverage]
        public AdminWalletController(IPointService service, 
            IMerchantService merchantSvc,
            IBankAccountService bankAccuntSvc)
        {
            svc = service;
            _merchantSvc = merchantSvc;
            _bankAccountSvc = bankAccuntSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddPoint/{orgId}/{walletId}")]
        public async Task<IActionResult> AddPoint(string orgId, string walletId, [FromBody] MPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.AddPoint(orgId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DeductPoint/{orgId}/{walletId}")]
        public async Task<IActionResult> DeductPoint(string orgId, string walletId, [FromBody] MPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.DeductPoint(orgId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPointTxsByWalletId/{orgId}/{walletId}")]
        public async Task<IActionResult> GetPointTxsByWalletId(string orgId, string walletId, [FromBody] VMPointTx request)
        {
            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }
Console.WriteLine($"DEBUG1 : Limit=[{request.Limit}], Offset=[{request.Offset}]");
            request.WalletId = walletId;
            var result = await svc.GetPointTxsByWalletId(orgId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPointTxsCountByWalletId/{orgId}/{walletId}")]
        public async Task<IActionResult> GetPointTxsCountByWalletId(string orgId, string walletId, [FromBody] VMPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.GetPointTxsCountByWalletId(orgId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPointBalanceByWalletId/{orgId}/{walletId}")]
        public async Task<IActionResult> GetPointBalanceByWalletId(string orgId, string walletId, [FromBody] VMPointBalance request)
        {
            request.WalletId = walletId;
            var result = await svc.GetPointBalanceByWalletId(orgId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetWalletByMerchantId/{merchantId}")]
        public async Task<IActionResult> GetWalletByMerchantId(string merchantId)
        {
            var vmMerchant = await _merchantSvc.GetMerchantById("global", merchantId);
            if (vmMerchant.Status != "OK")
            {
                return Ok(vmMerchant);
            }

            var mc = vmMerchant.Merchant!;

            var merchantOrgId = mc.OrgId!;
            var result = await svc.GetWalletByMerchantId(merchantOrgId, merchantId);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetWalletByBankAccountId/{bankAccountId}")]
        public async Task<IActionResult> GetWalletByBankAccountId(string bankAccountId)
        {
            var result = await svc.GetWalletByBankAccountId("global", bankAccountId);
            return Ok(result);
        }
    }
}
