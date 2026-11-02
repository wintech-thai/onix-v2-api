using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
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

        private async Task<MMerchant?> GetCurrentMerchant(string orgId)
        {
            var param = new VMMerchant { Limit = 100, Offset = 0 };
            var merchants = await _merchantSvc.GetMerchants(orgId, param);

            return merchants.FirstOrDefault(m => m.OrgId == orgId);
        }

        //x091 : เฉพาะ merchant แบบ MerchantOwn เท่านั้นที่จัดการ Pay-Out bank account ของตัวเองได้จากโปรแกรม Merchant
        private static MVBankAccount? DenyIfMerchantIsNotOwn(MMerchant? mc)
        {
            if (mc == null)
            {
                return new MVBankAccount { Status = "ERROR_MERCHANT_NOT_FOUND", Description = "Merchant not found for this organization" };
            }

            if (mc.PayInBankCustodyType != "MerchantOwn")
            {
                return new MVBankAccount { Status = "ERROR_PLATFORM_OWN_NOT_ALLOWED", Description = "This action is only allowed for MerchantOwn merchants" };
            }

            return null;
        }

        [HttpGet]
        [Route("org/{orgId}/action/GetAvailableBanks")]
        public IActionResult GetAvailableBanks(string orgId)
        {
            var result = _bankAccountSvc.GetAvailableBanks();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/AddPayOutBankAccount")]
        public async Task<IActionResult> AddPayOutBankAccount(string orgId, [FromBody] MBankAccount request)
        {
            var mc = await GetCurrentMerchant(orgId);
            var denyResult = DenyIfMerchantIsNotOwn(mc);
            if (denyResult != null)
            {
                return Ok(denyResult);
            }

            //บังคับค่าพวกนี้เอง ไม่เชื่อค่าที่ client ส่งมา — OrgId เป็นของ merchant เองเลย ไม่ใช่ "global" เหมือน bank account ฝั่ง Admin
            request.AccountCategory = "PayOut";
            request.CustodyType = "MerchantOwn";
            request.AccountLevel = "Selected";

            var result = await _bankAccountSvc.AddBankAccount(orgId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/UpdatePayOutBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> UpdatePayOutBankAccountById(string orgId, string bankAccountId, [FromBody] MBankAccount request)
        {
            var mc = await GetCurrentMerchant(orgId);
            var denyResult = DenyIfMerchantIsNotOwn(mc);
            if (denyResult != null)
            {
                return Ok(denyResult);
            }

            //UpdateBankAccountById ไม่แก้ BankCode อยู่แล้ว (ดู BankAccountRepository) จึง lock ฟิลด์ Bank ให้โดยอัตโนมัติ
            //และ scope ด้วย orgId ของ merchant เอง จึงแก้ได้แค่ bank account ของตัวเองเท่านั้น
            var result = await _bankAccountSvc.UpdateBankAccountById(orgId, bankAccountId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{orgId}/action/GetMyPayOutBankAccounts")]
        public async Task<IActionResult> GetMyPayOutBankAccounts(string orgId, [FromBody] VMBankAccount param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }
            param.AccountCategory = "PayOut";

            var result = await _bankAccountSvc.GetBankAccounts(orgId, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{orgId}/action/GetMyPayOutBankAccountCount")]
        public async Task<IActionResult> GetMyPayOutBankAccountCount(string orgId, [FromBody] VMBankAccount param)
        {
            param.AccountCategory = "PayOut";
            var result = await _bankAccountSvc.GetBankAccountCount(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyPayOutBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> GetMyPayOutBankAccountById(string orgId, string bankAccountId)
        {
            //GetBankAccountById scope ด้วย orgId อยู่แล้ว (BankAccountRepository filter ด้วย OrgId) จึงดึงได้แค่ของ org ตัวเองเท่านั้น
            var result = await _bankAccountSvc.GetBankAccountById(orgId, bankAccountId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/EnableMyPayOutBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> EnableMyPayOutBankAccountById(string orgId, string bankAccountId)
        {
            var mc = await GetCurrentMerchant(orgId);
            var denyResult = DenyIfMerchantIsNotOwn(mc);
            if (denyResult != null)
            {
                return Ok(denyResult);
            }

            var result = await _bankAccountSvc.UpdateBankAccountStatusById(orgId, bankAccountId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/DisableMyPayOutBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> DisableMyPayOutBankAccountById(string orgId, string bankAccountId)
        {
            var mc = await GetCurrentMerchant(orgId);
            var denyResult = DenyIfMerchantIsNotOwn(mc);
            if (denyResult != null)
            {
                return Ok(denyResult);
            }

            var result = await _bankAccountSvc.UpdateBankAccountStatusById(orgId, bankAccountId, "Disabled");
            return Ok(result);
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
