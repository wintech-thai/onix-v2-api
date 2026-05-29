using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminPaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService svc;
        private readonly IMerchantService _merchantSvc;
        private readonly IBankAccountService _bankAccountSvc;

        [ExcludeFromCodeCoverage]
        public AdminPaymentRequestController(
            IPaymentRequestService service, 
            IMerchantService merchantService,
            IBankAccountService bankAccountService)
        {
            svc = service;
            _merchantSvc = merchantService;
            _bankAccountSvc = bankAccountService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SubmitPaymentRequestByMerchantId/{merchantId}")]
        public async Task<IActionResult> SubmitPaymentRequestByMerchantId(string merchantId, [FromBody] MPaymentRequest request)
        {
            var mcVm = await _merchantSvc.GetMerchantById("notused", merchantId);
            if (mcVm.Status != "OK")
            {
                return Ok(mcVm);
            }

            var mc = mcVm.Merchant;
            if (mc == null)
            {
                return Ok(mcVm);
            }

            if (string.IsNullOrEmpty(mc.OrgId))
            {
                mcVm.Status = "ERROR_ORG_ID_EMPTY";
                mcVm.Description = "Organization ID is null or empty";
                return Ok(mcVm);
            }

            request.MerchantId = merchantId;
            request.MerchantId2 = Guid.Parse(merchantId);
            var result = await svc.AddPaymentRequestPayIn(mc.OrgId, request, mc);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInRequests")]
        public async Task<IActionResult> GetPayInRequests([FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentRequests("global", request);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayOutRequests")]
        public async Task<IActionResult> GetPayOutRequests([FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayOut";
            var result = await svc.GetPaymentRequests("global", request);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInRequestCount")]
        public async Task<IActionResult> GetPayInRequestCount([FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentRequestCount("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayOutRequestCount")]
        public async Task<IActionResult> GetPayOutRequestCount([FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayOut";
            var result = await svc.GetPaymentRequestCount("global", request);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentRequestById/{paymentRequestId}")]
        public async Task<IActionResult> GetPaymentRequestById(string paymentRequestId)
        {
            var result = await svc.GetPaymentRequestById("global", paymentRequestId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/CreatePayOutRequest")]
        public async Task<IActionResult> CreatePayOutRequest([FromBody] MPaymentRequest request)
        {
            var merchantId = request.MerchantId!;
            var bankAccountId = request.PayinBankAccountId!;

            var mcVm = await _merchantSvc.GetMerchantById("notused", merchantId);
            if (mcVm.Status != "OK")
            {
                return Ok(mcVm);
            }

            var mc = mcVm.Merchant;
            if (mc == null)
            {
                return Ok(mcVm);
            }

            if (string.IsNullOrEmpty(mc.OrgId))
            {
                mcVm.Status = "ERROR_ORG_ID_EMPTY";
                mcVm.Description = "Organization ID is null or empty";
                return Ok(mcVm);
            }

            //Bank Account
            var baVm = await _bankAccountSvc.GetBankAccountById("global", bankAccountId);
            if (baVm.Status != "OK")
            {
                return Ok(baVm);
            }

            var ba = baVm.BankAccount;
            if (ba == null)
            {
                return Ok(baVm);
            }

            request.MerchantId = merchantId;
            request.MerchantId2 = Guid.Parse(merchantId);
            var result = await svc.AddPaymentRequestPayOut(mc.OrgId, request, mc, ba);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdatePayOutRequestById/{paymentRequestId}")]
        public async Task<IActionResult> UpdatePayOutRequestById(string paymentRequestId, [FromBody] MPaymentRequest request)
        {
            //หน้าจอที่เรียก API นี้จะให้ update แต่บัญชีธนาคารที่จะจ่ายเงินออกเท่านั้น 
 
            var merchantId = request.MerchantId!;
            var bankAccountId = request.PayoutBankAccountId!; //บัญชีธนาคารที่จะเอาเงินออก (ซึ่งจะเป็น bank account ของ pool กลางที่ใช้สำหรับจ่ายเงินออกเท่านั้น ไม่ใช่ bank account ของ merchant)

            var mcVm = await _merchantSvc.GetMerchantById("notused", merchantId);
            if (mcVm.Status != "OK")
            {
                return Ok(mcVm);
            }

            var mc = mcVm.Merchant;
            if (mc == null)
            {
                return Ok(mcVm);
            }

            if (string.IsNullOrEmpty(mc.OrgId))
            {
                mcVm.Status = "ERROR_ORG_ID_EMPTY";
                mcVm.Description = "Organization ID is null or empty";
                return Ok(mcVm);
            }

            //Bank Account
            var baVm = await _bankAccountSvc.GetBankAccountById("global", bankAccountId);
            if (baVm.Status != "OK")
            {
                return Ok(baVm);
            }

            var ba = baVm.BankAccount;
            if (ba == null)
            {
                return Ok(baVm);
            }

            request.MerchantId = merchantId;
            request.MerchantId2 = Guid.Parse(merchantId);
            var result = await svc.UpdatePaymentRequestPayOut(mc.OrgId, paymentRequestId, request, ba, mc);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/RejectPayOutRequestById/{paymentRequestId}")]
        public async Task<IActionResult> RejectPayOutRequestById(string paymentRequestId, [FromBody] MPaymentRequest request)
        {
            var result = await svc.RejectPaymentRequestPayOut("global", paymentRequestId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/ApprovePayOutRequestById/{paymentRequestId}")]
        public async Task<IActionResult> ApprovePayOutRequestById(string paymentRequestId, [FromBody] MPaymentRequest request)
        {
            var result = await svc.ApprovePaymentRequestPayOut("global", paymentRequestId, request);
            return Ok(result);
        }
    }
}
