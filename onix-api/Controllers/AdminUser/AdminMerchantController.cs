using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminMerchantController : ControllerBase
    {
        private readonly IMerchantService svc;
        private readonly IBankAccountService _bankAccuntSvc;

        [ExcludeFromCodeCoverage]
        public AdminMerchantController(IMerchantService service, IBankAccountService bankAccountSvc)
        {
            svc = service;
            _bankAccuntSvc = bankAccountSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPayInBankAccountsForMerchant/{merchantId}")]
        public async Task<IActionResult> GetPayInBankAccountsForMerchant(string merchantId)
        {
            var result = await _bankAccuntSvc.GetPayInBankAccountsForMerchant("notused", merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetMerchantById/{merchantId}")]
        public async Task<IActionResult> GetMerchantById(string merchantId)
        {
            var result = await svc.GetMerchantById("notused", merchantId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetMerchantPaymentRequestEndPoint/{merchantId}")]
        public async Task<IActionResult> GetMerchantPaymentRequestEndPoint(string merchantId)
        {
            var mvMerchant = await svc.GetMerchantById("notused", merchantId);
            if (mvMerchant.Status != "OK")
            {
                return Ok(mvMerchant);
            }

            var mc = mvMerchant.Merchant!;

            var merchantOrgId = mc.OrgId;
            var url = $"https://<PAYMENT-REQUEST-SERVICE>/api/PaymentRequest/org/{merchantOrgId}/action/SubmitPaymentRequest";

            var result = new MVEndPoint()
            {
                Status = "OK",
                Description = "Success",
                PaymentRequestUrl = url,
            };

            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetMerchants")]
        public async Task<IActionResult> GetMerchants([FromBody] VMMerchant param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetMerchants("notused", param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetMerchantCount")]
        public async Task<IActionResult> GetMerchantCount([FromBody] VMMerchant param)
        {
            var result = await svc.GetMerchantCount("notused", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateMerchantById/{merchantId}")]
        public async Task<IActionResult> UpdateMerchantById(string merchantId, [FromBody] MMerchant request)
        {
            var result = await svc.UpdateMerchantById("notused", merchantId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableMerchantById/{merchantId}")]
        public async Task<IActionResult> EnableMerchantById(string merchantId)
        {
            var result = await svc.UpdateMerchantStatusById(merchantId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableMerchantById/{merchantId}")]
        public async Task<IActionResult> DisableMerchantById(string merchantId)
        {
            var result = await svc.UpdateMerchantStatusById(merchantId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
    }
}
