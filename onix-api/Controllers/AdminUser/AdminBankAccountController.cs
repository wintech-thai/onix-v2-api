using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminBankAccountController : ControllerBase
    {
        private readonly IBankAccountService svc;

        [ExcludeFromCodeCoverage]
        public AdminBankAccountController(IBankAccountService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> GetBankAccountById(string bankAccountId)
        {
            var result = await svc.GetBankAccountById("global", bankAccountId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetBankAccounts")]
        public async Task<IActionResult> GetBankAccounts([FromBody] VMBankAccount param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetBankAccounts("global", param);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetAvailableBanks")]
        public IActionResult GetAvailableBanks()
        {
            var result = svc.GetAvailableBanks();
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetAvailableSupportQrBanks")]
        public IActionResult GetAvailableSupportQrBanks()
        {
            var result = svc.GetAvailableSupportQrBanks();
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetBankAccountCount")]
        public async Task<IActionResult> GetBankAccountCount([FromBody] VMBankAccount param)
        {
            var result = await svc.GetBankAccountCount("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> UpdateBankAccountById(string bankAccountId, [FromBody] MBankAccount request)
        {
            var result = await svc.UpdateBankAccountById("global", bankAccountId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> DeleteBankAccountById(string bankAccountId)
        {
            var result = await svc.DeleteBankAccountById("global", bankAccountId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddBankAccount")]
        public async Task<IActionResult> AddBankAccount([FromBody] MBankAccount request)
        {
            var result = await svc.AddBankAccount("global", request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> EnableBankAccountById(string bankAccountId)
        {
            var result = await svc.UpdateBankAccountStatusById("global", bankAccountId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableBankAccountById/{bankAccountId}")]
        public async Task<IActionResult> DisableBankAccountById(string bankAccountId)
        {
            var result = await svc.UpdateBankAccountStatusById("global", bankAccountId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetMerchantsForBankAccount/{bankAccountId}")]
        public async Task<IActionResult> GetMerchantsForBankAccount(string bankAccountId)
        {
            var result = await svc.GetMerchantsForBankAccount("global", bankAccountId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPayInBankAccountsForMerchant/{merchantId}")]
        public async Task<IActionResult> GetPayInBankAccountsForMerchant(string merchantId)
        {
            var result = await svc.GetPayInBankAccountsForMerchant("global", merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SelectMerchantById/{bankAccountId}/{merchantId}")]
        public async Task<IActionResult> SelectMerchantById(string bankAccountId, string merchantId)
        {
            var result = await svc.SelectMerchant("global", bankAccountId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UnSelectMerchantById/{bankAccountId}/{merchantId}")]
        public async Task<IActionResult> UnSelectMerchantById(string bankAccountId, string merchantId)
        {
            var result = await svc.UnSelectMerchant("global", bankAccountId, merchantId);
            return Ok(result);
        }
    }
}
