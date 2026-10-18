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
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountSvc;
        private readonly IMerchantService _merchantSvc;
        private readonly IPointService _pointSvc;

        [ExcludeFromCodeCoverage]
        public BankAccountController(IBankAccountService bankAccountSvc, IMerchantService merchantSvc, IPointService pointSvc)
        {
            _bankAccountSvc = bankAccountSvc;
            _merchantSvc = merchantSvc;
            _pointSvc = pointSvc;
        }

        private async Task<string?> GetCurrentMerchantId(string requestedOrgId)
        {
            if (string.IsNullOrEmpty(requestedOrgId))
            {
                return null;
            }

            var param = new VMMerchant { Limit = 100, Offset = 0 };
            var merchants = await _merchantSvc.GetMerchants(requestedOrgId, param);

            return merchants.FirstOrDefault(m => m.OrgId == requestedOrgId)?.Id?.ToString();
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPayInBankAccountsForMerchant")]
        public async Task<IActionResult> GetPayInBankAccountsForMerchant(string orgId)
        {
            var param = new VMMerchant { Limit = 1, Offset = 0, FullTextSearch = orgId };
            var merchants = await _merchantSvc.GetMerchants(orgId, param);

            if (merchants == null || merchants.Count == 0)
            {
                return Ok(new { Status = "Error", Description = "No merchant found for this org" });
            }

            var merchantId = merchants[0].Id!.ToString()!;

            var result = await _bankAccountSvc.GetPayInBankAccountsWithGlobalForMerchant(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPayOutBankAccountsForMerchant")]
        public async Task<IActionResult> GetPayOutBankAccountsForMerchant(string orgId)
        {
            var param = new VMMerchant { Limit = 1, Offset = 0, FullTextSearch = orgId };
            var merchants = await _merchantSvc.GetMerchants(orgId, param);

            if (merchants == null || merchants.Count == 0)
            {
                return Ok(new { Status = "Error", Description = "No merchant found for this org" });
            }

            var merchantId = merchants[0].Id!.ToString()!;
            var result = await _bankAccountSvc.GetPayOutBankAccountsForMerchant(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetAvailableSupportQrBanks")]
        public IActionResult GetAvailableSupportQrBanks(string orgId)
        {
            var result = _bankAccountSvc.GetAvailableSupportQrBanks();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyBankAccounts")]
        public async Task<IActionResult> GetMyBankAccounts(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

            var result = await _bankAccountSvc.GetPayInBankAccountsForMerchant(orgId, merchantId);
            return Ok(result);
        }

        private async Task<bool> IsBankAccountLinkedToMerchant(string orgId, string bankAccountId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return false;
            }

            var linked = await _bankAccountSvc.GetPayInBankAccountsForMerchant(orgId, merchantId);
            return linked.Any(p => p.BankAccountId == bankAccountId);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> GetMyBankAccountById(string orgId, string bankAccountId)
        {
            var isLinked = await IsBankAccountLinkedToMerchant(orgId, bankAccountId);
            if (!isLinked)
            {
                return Ok(new { Status = "FORBIDDEN", Description = "Bank account is not linked to this merchant" });
            }

            var result = await _bankAccountSvc.GetBankAccountById("global", bankAccountId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyBankAccountWallet/{bankAccountId}")]
        public async Task<IActionResult> GetMyBankAccountWallet(string orgId, string bankAccountId)
        {
            var isLinked = await IsBankAccountLinkedToMerchant(orgId, bankAccountId);
            if (!isLinked)
            {
                return Ok(new { Status = "FORBIDDEN", Description = "Bank account is not linked to this merchant" });
            }

            var result = await _pointSvc.GetWalletByBankAccountId("global", bankAccountId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetMyBankAccountWalletTxs/{bankAccountId}")]
        public async Task<IActionResult> GetMyBankAccountWalletTxs(string orgId, string bankAccountId, [FromBody] VMPointTx request)
        {
            var isLinked = await IsBankAccountLinkedToMerchant(orgId, bankAccountId);
            if (!isLinked)
            {
                return Ok(new { Status = "FORBIDDEN", Description = "Bank account is not linked to this merchant" });
            }

            var wallet = await _pointSvc.GetWalletByBankAccountId("global", bankAccountId);
            if (wallet?.Wallet?.Id == null)
            {
                return Ok(new { Status = "NOTFOUND", Description = "Wallet not found" });
            }

            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }
            request.WalletId = wallet.Wallet.Id.ToString();

            var result = await _pointSvc.GetPointTxsByWalletId("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetMyBankAccountWalletTxsCount/{bankAccountId}")]
        public async Task<IActionResult> GetMyBankAccountWalletTxsCount(string orgId, string bankAccountId, [FromBody] VMPointTx request)
        {
            var isLinked = await IsBankAccountLinkedToMerchant(orgId, bankAccountId);
            if (!isLinked)
            {
                return Ok(new { Status = "FORBIDDEN", Description = "Bank account is not linked to this merchant" });
            }

            var wallet = await _pointSvc.GetWalletByBankAccountId("global", bankAccountId);
            if (wallet?.Wallet?.Id == null)
            {
                return Ok(new { Status = "NOTFOUND", Description = "Wallet not found" });
            }

            request.WalletId = wallet.Wallet.Id.ToString();

            var result = await _pointSvc.GetPointTxsCountByWalletId("global", request);
            return Ok(result);
        }
    }
}
