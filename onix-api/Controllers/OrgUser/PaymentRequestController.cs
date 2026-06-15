using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService _paymentRequestSvc;
        private readonly IMerchantService _merchantSvc;
        private readonly IBankAccountService _bankAccountSvc;

        [ExcludeFromCodeCoverage]
        public PaymentRequestController(IPaymentRequestService paymentRequestSvc, IMerchantService merchantSvc, IBankAccountService bankAccountSvc)
        {
            _paymentRequestSvc = paymentRequestSvc;
            _merchantSvc = merchantSvc;
            _bankAccountSvc = bankAccountSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentRequests")]
        public async Task<IActionResult> GetPaymentRequests(string orgId, [FromBody] VMPaymentRequest param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await _paymentRequestSvc.GetPaymentRequests(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentRequestCount")]
        public async Task<IActionResult> GetPaymentRequestCount(string orgId, [FromBody] VMPaymentRequest param)
        {
            var result = await _paymentRequestSvc.GetPaymentRequestCount(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPaymentRequestById/{paymentRequestId}")]
        public async Task<IActionResult> GetPaymentRequestById(string orgId, string paymentRequestId)
        {
            var result = await _paymentRequestSvc.GetPaymentRequestById(orgId, paymentRequestId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPayOutRequestById/{paymentRequestId}")]
        public async Task<IActionResult> GetPayOutRequestById(string orgId, string paymentRequestId)
        {
            var result = await _paymentRequestSvc.GetPaymentRequestById(orgId, paymentRequestId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/CreatePayOutRequest")]
        public async Task<IActionResult> CreatePayOutRequest(string orgId, [FromBody] MPaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.MerchantId))
                return Ok(new { Status = "Error", Description = "MerchantId is required" });

            var merchantVm = await _merchantSvc.GetMerchantById(orgId, request.MerchantId);
            if (merchantVm.Status != "OK" || merchantVm.Merchant == null)
                return Ok(merchantVm);

            var merchant = merchantVm.Merchant;
            var bankAccountId = request.PayinBankAccountId!;
            var baVm = new MVBankAccount() { Status = "OK", BankAccount = null };
            if (string.IsNullOrEmpty(bankAccountId))
            {
                baVm.BankAccount = new MBankAccount()
                {
                    Id = null,
                    BankCode = request.BankCode!,
                    AccountNumber = request.BankAccountNo!,
                    AccountName = request.BankAccountName!,
                    PromptPayId = request.PromptPayId!,
                    AccountType = !string.IsNullOrEmpty(request.PromptPayId) ? "PromptPay" : "Bank",
                };
            }
            else
            {
                baVm = await _bankAccountSvc.GetBankAccountById("global", bankAccountId);
                if (baVm.Status != "OK" || baVm.BankAccount == null)
                    return Ok(baVm);
            }

            request.MerchantId = merchant.Id!.ToString();
            request.MerchantId2 = merchant.Id ?? Guid.Empty;
            request.Direction = "PayOut";
            request.Currency = "THB";

            var result = await _paymentRequestSvc.AddPaymentRequestPayOut(orgId, request, merchant, baVm.BankAccount);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/UpdatePayOutRequestById/{paymentRequestId}")]
        public async Task<IActionResult> UpdatePayOutRequestById(string orgId, string paymentRequestId, [FromBody] MPaymentRequest request)
        {
            if (!string.IsNullOrEmpty(request.PayinBankAccountId))
            {
                var baVm = await _bankAccountSvc.GetBankAccountById("global", request.PayinBankAccountId);
                if (baVm.Status == "OK" && baVm.BankAccount != null)
                {
                    var ba = baVm.BankAccount;
                    request.PayinBankCode = ba.BankCode;
                    request.PayinBankAccountNo = ba.AccountNumber;
                    request.PayinBankAccountName = ba.AccountName;
                    request.PayinPromptPayId = ba.PromptPayId;
                    request.PayinAccountType = ba.AccountType;
                }
            }
            var result = await _paymentRequestSvc.UpdatePaymentRequestById(orgId, paymentRequestId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/DeletePayOutRequestById/{paymentRequestId}")]
        public async Task<IActionResult> DeletePayOutRequestById(string orgId, string paymentRequestId)
        {
            var result = await _paymentRequestSvc.DeletePayOutRequestById(orgId, paymentRequestId);
            return Ok(result);
        }


        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/SubmitPayInRequest/{merchantId}")]
        public async Task<IActionResult> SubmitPayInRequest(string orgId, string merchantId, [FromBody] MPaymentRequest request)
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
            var result = await _paymentRequestSvc.AddPaymentRequestPayIn(orgId, request, mc);

            result.PaymentResponse!.QrCodeImage = "";

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/SubmitPayOutRequest/{merchantId}")]
        public async Task<IActionResult> SubmitPayOutRequest(string orgId, string merchantId, [FromBody] MPaymentRequest request)
        {
            //เอาไว้ให้ merchant เป็นคนเรียกเพื่อของ pay-out
            var merchantVm = await _merchantSvc.GetMerchantById(orgId, merchantId);
            if (merchantVm.Status != "OK" || merchantVm.Merchant == null)
                return Ok(merchantVm);

            var merchant = merchantVm.Merchant;

            var bankAccountId = request.PayinBankAccountId!;
            var baVm = new MVBankAccount() { Status = "OK", BankAccount = null };
            if (string.IsNullOrEmpty(bankAccountId))
            {
                //merchant จะส่ง bank account no, name ใหม่มา override เอง โดยจะไม่ใช้ PayinBankAccountId
                baVm.BankAccount = new MBankAccount()
                {
                    Id = null, //กรณีที่ไม่ระบุ PayinBankAccountId มา จะใช้ข้อมูลบัญชีที่ส่งมาใน request แทน โดยจะไม่เชื่อมโยงกับ BankAccount จริงๆ ในระบบ
                    BankCode = request.BankCode!,
                    AccountNumber = request.BankAccountNo!,
                    AccountName = request.BankAccountName!,
                    PromptPayId = request.PromptPayId!,
                    AccountType = request.AccountType!, //Native หรือ PromptPay
                };
            }
            else
            {
                baVm = await _bankAccountSvc.GetBankAccountById("global", bankAccountId);
                if (baVm.Status != "OK" || baVm.BankAccount == null)
                return Ok(baVm);
            }

            request.MerchantId = merchant.Id!.ToString();
            request.MerchantId2 = merchant.Id ?? Guid.Empty;
            request.Direction = "PayOut";
            request.Currency = "THB";

            var result = await _paymentRequestSvc.AddPaymentRequestPayOut(orgId, request, merchant, baVm.BankAccount);
            return Ok(result);
        }
    }
}
