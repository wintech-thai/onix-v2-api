using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminMerchantCurrencyController : ControllerBase
    {
        private readonly IMerchantCurrencyService svc;
        //private readonly IBankAccountService _bankAccuntSvc;

        [ExcludeFromCodeCoverage]
        public AdminMerchantCurrencyController(IMerchantCurrencyService service)
        {
            svc = service;
            //_bankAccuntSvc = bankAccountSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetAvailableFiatCurrencies")]
        public async Task<IActionResult> GetAvailableFiatCurrencies()
        {
            var result = svc.GetAvailableCurrencies("FIAT");
            await Task.CompletedTask;

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetAvailableCryptoCurrencies")]
        public async Task<IActionResult> GetAvailableCryptoCurrencies()
        {
            var result = svc.GetAvailableCurrencies("CRYPTO");
            await Task.CompletedTask;
            
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetMerchantCurrencyById/{merchantCurrencyId}")]
        public async Task<IActionResult> GetMerchantCurrencyById(string merchantCurrencyId)
        {
            var result = await svc.GetCurrencyById("global", merchantCurrencyId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetCurrenciesByMerchantId/{merchantId}")]
        public async Task<IActionResult> GetCurrenciesByMerchantId(string merchantId)
        {
            var arr = await svc.GetCurrenciesByMerchantId("global", merchantId);
            return Ok(arr);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateMerchantCurrencyById/{merchantCurrencyId}")]
        public async Task<IActionResult> UpdateMerchantCurrencyById(string merchantCurrencyId, [FromBody] MMerchantCurrency request)
        {
            var result = await svc.UpdateCurrencyById("global", merchantCurrencyId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddMerchantCurrency")]
        public async Task<IActionResult> AddMerchantCurrency([FromBody] MMerchantCurrency request)
        {
            var result = await svc.AddCurrency("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableMerchantCurrency/{merchantCurrencyId}")]
        public async Task<IActionResult> EnableMerchantCurrency(string merchantCurrencyId)
        {
            var result = await svc.UpdateCurrencyStatusById("global", merchantCurrencyId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableMerchantCurrency/{merchantCurrencyId}")]
        public async Task<IActionResult> DisableMerchantCurrency(string merchantCurrencyId)
        {
            var result = await svc.UpdateCurrencyStatusById("global", merchantCurrencyId, "Disabled");
            return Ok(result);
        }
    }
}
