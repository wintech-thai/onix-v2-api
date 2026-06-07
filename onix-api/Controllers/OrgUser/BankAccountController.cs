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

        [ExcludeFromCodeCoverage]
        public BankAccountController(IBankAccountService bankAccountSvc, IMerchantService merchantSvc)
        {
            _bankAccountSvc = bankAccountSvc;
            _merchantSvc = merchantSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPayInBankAccountsForMerchant")]
        public async Task<IActionResult> GetPayInBankAccountsForMerchant(string orgId)
        {
            var param = new VMMerchant { Limit = 1, Offset = 0 };
            var merchants = await _merchantSvc.GetMerchants(orgId, param);
            
            if (merchants == null || merchants.Count == 0)
            {
                return Ok(new { Status = "Error", Description = "No merchant found for this org" });
            }

            var merchantId = merchants[0].Id!.ToString();
            var result = await _bankAccountSvc.GetPayInBankAccountsForMerchant(orgId, merchantId);
            return Ok(result);
        }
    }
}
