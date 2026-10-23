using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Models;
using System.Security.Claims;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PaymentTransactionController : ControllerBase
    {
        private readonly IPaymentTransactionService _paymentTransactionSvc;
        private readonly IMerchantService _merchantSvc;
        private readonly IBankAccountService _bankAccountSvc;
        private readonly IJobService _jobService;

        [ExcludeFromCodeCoverage]
        public PaymentTransactionController(
            IPaymentTransactionService paymentTransactionSvc,
            IMerchantService merchantSvc,
            IBankAccountService bankAccountSvc,
            IJobService jobService)
        {
            _paymentTransactionSvc = paymentTransactionSvc;
            _merchantSvc = merchantSvc;
            _bankAccountSvc = bankAccountSvc;
            _jobService = jobService;
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

        private async Task<List<string>> GetMyBankAccountIds(string orgId, string merchantId)
        {
            var accounts = await _bankAccountSvc.GetPayInBankAccountsForMerchant(orgId, merchantId);
            return accounts
                .Select(a => a.BankAccountId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => id!)
                .ToList();
        }

        //x088 : merchant แบบ MerchantOwn ให้เห็น Unidentified tx ของ bank account ตัวเองได้ด้วย แม้ OrgId ยังเป็น global อยู่ (ยังไม่ได้ approve)
        private async Task ApplyUnidentifiedVisibility(string orgId, VMPaymentTransaction param)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return;
            }

            var mcVm = await _merchantSvc.GetMerchantById(orgId, merchantId);
            if (mcVm.Status != "OK" || mcVm.Merchant?.PayInBankCustodyType != "MerchantOwn")
            {
                return;
            }

            param.IncludeUnidentifiedForBankAccountIds = await GetMyBankAccountIds(orgId, merchantId);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentTransactions")]
        public async Task<IActionResult> GetPaymentTransactions(string orgId, [FromBody] VMPaymentTransaction param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            await ApplyUnidentifiedVisibility(orgId, param);

            var result = await _paymentTransactionSvc.GetPaymentTransactions(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentTransactionCount")]
        public async Task<IActionResult> GetPaymentTransactionCount(string orgId, [FromBody] VMPaymentTransaction param)
        {
            await ApplyUnidentifiedVisibility(orgId, param);

            var result = await _paymentTransactionSvc.GetPaymentTransactionCount(orgId, param);
            return Ok(result);
        }

        //x088 : merchant แบบ MerchantOwn เท่านั้น approve/reject Unidentified tx ของตัวเองได้ (ไม่ต้องเลือก merchant เพราะรู้อยู่แล้วจาก orgId)
        //ตัว service เดิมใช้ orgId="global" ในการค้นหา record เพราะ Unidentified tx ยังไม่มี OrgId เป็นของ merchant
        //เลยต้องเช็คเองตรงนี้ก่อนว่า PayInBankAccountId ของ tx ที่จะ approve/reject เป็นของ merchant ที่ login อยู่จริง ๆ
        //ไม่งั้น merchant อื่นจะเดา paymentTransactionId แล้วไป reject/approve ของ merchant คนอื่นได้ (RejectUnidentifiedPaymentTx เดิมไม่มีการเช็คความเป็นเจ้าของ bank account เลย)
        private async Task<(string? merchantId, MVPaymentTransaction? denyResult)> AuthorizeMerchantOwnUnidentifiedTx(string orgId, string paymentTransactionId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return (null, new MVPaymentTransaction { Status = "ERROR_MERCHANT_NOT_FOUND", Description = "Merchant not found" });
            }

            var mcVm = await _merchantSvc.GetMerchantById(orgId, merchantId);
            if (mcVm.Status != "OK")
            {
                return (null, new MVPaymentTransaction { Status = mcVm.Status, Description = mcVm.Description });
            }

            if (mcVm.Merchant?.PayInBankCustodyType != "MerchantOwn")
            {
                return (null, new MVPaymentTransaction
                {
                    Status = "ERROR_PLATFORM_OWN_NOT_ALLOWED",
                    Description = "This merchant is PlatformOwn custody type, this action is only allowed from the Admin app"
                });
            }

            var txVm = await _paymentTransactionSvc.GetPaymentTransactionById("global", paymentTransactionId);
            if (txVm.Status != "OK")
            {
                return (null, txVm);
            }

            var myBankAccountIds = await GetMyBankAccountIds(orgId, merchantId);
            if (!myBankAccountIds.Contains(txVm.PaymentTransaction?.PayInBankAccountId ?? ""))
            {
                return (null, new MVPaymentTransaction
                {
                    Status = "ERROR_MERCHANT_NOT_ALLOW_BANK_ACCOUNT",
                    Description = $"Payment Transaction ID [{paymentTransactionId}] does not belong to a bank account of Merchant ID [{merchantId}]"
                });
            }

            return (merchantId, null);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/ApproveUnidentifiedPaymentTx/{paymentTransactionId}")]
        public async Task<IActionResult> ApproveUnidentifiedPaymentTx(string orgId, string paymentTransactionId)
        {
            var (merchantId, denyResult) = await AuthorizeMerchantOwnUnidentifiedTx(orgId, paymentTransactionId);
            if (denyResult != null)
            {
                return Ok(denyResult);
            }

            var result = await _paymentTransactionSvc.ApproveUnidentifiedPaymentTx("global", paymentTransactionId, merchantId!);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/RejectUnidentifiedPaymentTx/{paymentTransactionId}")]
        public async Task<IActionResult> RejectUnidentifiedPaymentTx(string orgId, string paymentTransactionId, [FromBody] MPaymentTransaction request)
        {
            var (_, denyResult) = await AuthorizeMerchantOwnUnidentifiedTx(orgId, paymentTransactionId);
            if (denyResult != null)
            {
                return Ok(denyResult);
            }

            var result = await _paymentTransactionSvc.RejectUnidentifiedPaymentTx("global", paymentTransactionId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPaymentTransactionById/{paymentTransactionId}")]
        public async Task<IActionResult> GetPaymentTransactionById(string orgId, string paymentTransactionId)
        {
            var result = await _paymentTransactionSvc.GetPaymentTransactionById(orgId, paymentTransactionId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPaymentTransactionJobById/{paymentTransactionId}/{jobId}")]
        public async Task<IActionResult> GetPaymentTransactionJobById(string orgId, string paymentTransactionId, string jobId)
        {
            var pmtVm = await _paymentTransactionSvc.GetPaymentTransactionById(orgId, paymentTransactionId);
            if (pmtVm.Status != "OK")
            {
                return Ok(pmtVm);
            }

            var pmt = pmtVm.PaymentTransaction!;
            var result = _jobService.GetJobById(pmt.OrgId!, jobId);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/CreatePaymentTxByPayInRequestId/{paymentRequestId}")]
        public async Task<IActionResult> CreatePaymentTxByPayInRequestId(string orgId, string paymentRequestId, [FromBody] MPaymentRequest? payload = null)
        {
            //orgId มาจาก route ของ OrgUser เอง จึงจำกัดได้แค่ payment request ของ org ตัวเองเท่านั้น (เหมือนกับ endpoint อื่น ๆ ของ OrgUser)
            var result = await _paymentTransactionSvc.CreatePaymentTxByPayInRequestId(orgId, paymentRequestId, payload);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPendingPayInRequestsForPaymentTx")]
        public async Task<IActionResult> GetPendingPayInRequestsForPaymentTx(string orgId, [FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayIn";
            request.Status = "Pending";
            request.FromDate = DateTime.UtcNow.AddHours(-1);
            var paymentRequests = await _paymentTransactionSvc.GetPaymentRequestsForPaymentTx("global", request);
            return Ok(paymentRequests);
        }
    }
}
