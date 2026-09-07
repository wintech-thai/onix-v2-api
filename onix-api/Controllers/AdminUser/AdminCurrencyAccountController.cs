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
    public class AdminCurrencyAccountController : ControllerBase
    {
        private readonly ICurrencyAccountService svc;

        [ExcludeFromCodeCoverage]
        public AdminCurrencyAccountController(ICurrencyAccountService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetCurrencyAccountById/{currencyAccountId}")]
        public async Task<IActionResult> GetCurrencyAccountById(string currencyAccountId)
        {
            var result = await svc.GetCurrencyAccountById("global", currencyAccountId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetCurrencyAccounts")]
        public async Task<IActionResult> GetCurrencyAccounts([FromBody] VMCurrencyAccount param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetCurrencyAccounts("global", param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetCurrencyAccountCount")]
        public async Task<IActionResult> GetCurrencyAccountCount([FromBody] VMCurrencyAccount param)
        {
            var result = await svc.GetCurrencyAccountCount("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddCurrencyAccount")]
        public async Task<IActionResult> AddCurrencyAccount([FromBody] MCurrencyAccount request)
        {
            var result = (request.CurrencyCategory == "CRYPTO")
                ? await svc.AddCryptoCurrencyAccount("global", request)
                : await svc.AddFiatCurrencyAccount("global", request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateCurrencyAccountById/{currencyAccountId}")]
        public async Task<IActionResult> UpdateCurrencyAccountById(string currencyAccountId, [FromBody] MCurrencyAccount request)
        {
            var result = await svc.UpdateCurrencyAccountById("global", currencyAccountId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteCurrencyAccountById/{currencyAccountId}")]
        public async Task<IActionResult> DeleteCurrencyAccountById(string currencyAccountId)
        {
            var result = await svc.DeleteCurrencyAccountById("global", currencyAccountId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableCurrencyAccountById/{currencyAccountId}")]
        public async Task<IActionResult> EnableCurrencyAccountById(string currencyAccountId)
        {
            var result = await svc.UpdateCurrencyAccountStatusById("global", currencyAccountId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableCurrencyAccountById/{currencyAccountId}")]
        public async Task<IActionResult> DisableCurrencyAccountById(string currencyAccountId)
        {
            var result = await svc.UpdateCurrencyAccountStatusById("global", currencyAccountId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
    }
}
