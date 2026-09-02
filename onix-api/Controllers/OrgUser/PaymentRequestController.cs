using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

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
        private readonly IJobService _jobService;

        [ExcludeFromCodeCoverage]
        public PaymentRequestController(IPaymentRequestService paymentRequestSvc, IMerchantService merchantSvc, IBankAccountService bankAccountSvc, IJobService jobService)
        {
            _paymentRequestSvc = paymentRequestSvc;
            _merchantSvc = merchantSvc;
            _bankAccountSvc = bankAccountSvc;
            _jobService = jobService;
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
        [Route("org/{orgId}/action/SubmitPayInRequestP2P/{merchantId}")]
        public async Task<IActionResult> SubmitPayInRequestP2P(string orgId, string merchantId, [FromBody] MPaymentRequest request)
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
            var result = await _paymentRequestSvc.AddPaymentRequestPayInP2P(orgId, request, mc);

            result.PaymentResponse!.QrCodeImage = "";

            if (result.Status == "OK")
            {
                //มี payout ให้ match ได้
                return Ok(result);
            }

            if (mc.PayoutNotMatchActionP2P == "UseNative")
            {
                //ให้ใช้ bank account กลาง
                var result2 = await _paymentRequestSvc.AddPaymentRequestPayIn(orgId, request, mc);
                result2.PaymentResponse!.QrCodeImage = "";
                return Ok(result2);
            }

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

        [HttpGet]
        [Route("org/{orgId}/action/GetPaymentRequestJobById/{paymentRequestId}/{jobId}")]
        public async Task<IActionResult> GetPaymentRequestJobById(string orgId, string paymentRequestId, string jobId)
        {
            var pmtVm = await _paymentRequestSvc.GetPaymentRequestById(orgId, paymentRequestId);
            if (pmtVm.Status != "OK")
            {
                return Ok(pmtVm);
            }

            var pmt = pmtVm.PaymentRequest!;
            var result = _jobService.GetJobById(pmt.OrgId!, jobId);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("org/{orgId}/action/VerifyPayInToken/{paymentRequestId}/{token}")]
        public async Task<IActionResult> VerifyPayInToken(string orgId, string paymentRequestId, string token)
        {
            var result = await _paymentRequestSvc.VerifyPayInSlipToken(paymentRequestId, token);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("org/{orgId}/action/UploadPayInSlipById/{paymentRequestId}/{token}")]
        public async Task<IActionResult> UploadPayInSlipById(string orgId, string paymentRequestId, string token, [FromBody] VMUploadPayInSlip payload)
        {
            var result = await _paymentRequestSvc.UploadPayInSlipById(paymentRequestId, token, payload.ImageBase64!, payload.First4, payload.Last4, payload.Note);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("org/{orgId}/action/CheckPayInSlipDup/{paymentRequestId}/{first4}/{last4}")]
        public IActionResult CheckPayInSlipDup(string orgId, string paymentRequestId, string first4, string last4)
        {
            var dups = _paymentRequestSvc.CheckPayInSlipDup(first4, last4, paymentRequestId);
            return Ok(new { Status = "OK", Duplicates = dups });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("org/{orgId}/action/VerifyPayOutToken/{paymentRequestId}/{token}")]
        public async Task<IActionResult> VerifyPayOutToken(string orgId, string paymentRequestId, string token)
        {
            var result = await _paymentRequestSvc.VerifyPayOutSlipToken(paymentRequestId, token);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("org/{orgId}/action/UploadPayOutSlipById/{paymentRequestId}/{token}")]
        public async Task<IActionResult> UploadPayOutSlipById(string orgId, string paymentRequestId, string token, [FromBody] VMUploadPayInSlip payload)
        {
            var result = await _paymentRequestSvc.UploadPayOutSlipById(paymentRequestId, token, payload.ImageBase64!, payload.First4, payload.Last4, payload.Note);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("org/{orgId}/action/CheckPayOutSlipDup/{paymentRequestId}/{first4}/{last4}")]
        public IActionResult CheckPayOutSlipDup(string orgId, string paymentRequestId, string first4, string last4)
        {
            var dups = _paymentRequestSvc.CheckPayOutSlipDup(first4, last4, paymentRequestId);
            return Ok(new { Status = "OK", Duplicates = dups });
        }

        [HttpGet]
        [Route("org/{orgId}/action/GetPayInSlipUpload/{paymentRequestId}")]
        public async Task<IActionResult> GetPayInSlipUpload(string orgId, string paymentRequestId)
        {
            var result = await _paymentRequestSvc.GetPayInSlipUploads(orgId, paymentRequestId);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{orgId}/action/GetPayOutSlipUpload/{paymentRequestId}")]
        public async Task<IActionResult> GetPayOutSlipUpload(string orgId, string paymentRequestId)
        {
            var result = await _paymentRequestSvc.GetPayOutSlipUploads(orgId, paymentRequestId);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{orgId}/action/GeneratePayInSlipUploadToken/{paymentRequestId}")]
        public async Task<IActionResult> GeneratePayInSlipUploadToken(string orgId, string paymentRequestId)
        {
            var result = await _paymentRequestSvc.GeneratePayInSlipUploadToken(orgId, paymentRequestId);
            return Ok(result);
        }
    }
}
