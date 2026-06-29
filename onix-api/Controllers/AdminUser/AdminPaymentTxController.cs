using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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
    public class AdminPaymentTxController : ControllerBase
    {
        private readonly IPaymentTransactionService svc;
        private readonly IJobService _jobService;

        [ExcludeFromCodeCoverage]
        public AdminPaymentTxController(IPaymentTransactionService service, IJobService jobService)
        {
            svc = service;
            _jobService = jobService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SubmitLinePaymentTxNotification/{bankAccountId}")] 
        public async Task<IActionResult> SubmitLinePaymentTxNotification(string bankAccountId, [FromBody] MPaymentNotiLine request)
        {
            var result = await svc.ProcessLinePaymentTxNotification("global", bankAccountId, request);
            return Ok(result);
        }

        private static MPaymentNotiLine GetPaymentNotiObj(Dictionary<string, object> bd)
        {
            bd.TryGetValue("amount", out var amount);
            amount ??= 0.00;

            bd.TryGetValue("billPaymentRef1", out var billPaymentRef1);
            billPaymentRef1 ??= "";

            var pmt = new MPaymentNotiLine()
            {
                TxType = "PayIn",
                RefId1 = billPaymentRef1.ToString(),
                PaymentAmount = (decimal?) amount,
                OriginalData = bd,
            };

            return pmt; 
        }

        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpPost]
        [Route("org/global/action/SubmitScbPaymentConfirmation/{bankAccountId}")]
        public async Task<IActionResult> SubmitScbPaymentConfirmation(string bankAccountId, [FromBody] Dictionary<string, object> request)
        {
            //TODO : เอา api secret มา verify digital signature ด้วยเพื่อมั่นใจว่าส่งมาจาก SCB จริง ๆ
            var dump = JsonSerializer.Serialize(request);
            Console.WriteLine($"INFO : [SubmitScbPaymentConfirmation] bankAccountId=[{bankAccountId}] : {dump}");

            var pmtNoti = GetPaymentNotiObj(request);
            var _ = await svc.ProcessLinePaymentTxNotification("global", bankAccountId, pmtNoti);

            //ถ้าไม่ echo กลับ SCB จะมองว่า merchant ไม่ได้รับ confirmation แล้วยิง webhook ซ้ำสูงสุด 3 ครั้ง
            var transactionId = request.TryGetValue("transactionId", out var txIdObj) && txIdObj is JsonElement txIdEl
                ? txIdEl.GetString()
                : null;

            return Ok(new { resCode = "00", resDesc = "success", transactionId });
        }

        [HttpPost]
        [Route("org/global/action/GetPayInTransactions")]
        public async Task<IActionResult> GetPayInTransactions([FromBody] VMPaymentTransaction request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentTransactions("global", request);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayOutTransactions")]
        public async Task<IActionResult> GetPayOutTransactions([FromBody] VMPaymentTransaction request)
        {
            request.Direction = "PayOut";
            var result = await svc.GetPaymentTransactions("global", request);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentTransactionById/{paymentTransactionId}")]
        public async Task<IActionResult> GetPaymentTransactionById(string paymentTransactionId)
        {
            var result = await svc.GetPaymentTransactionById("global", paymentTransactionId);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentTransactionJobById/{paymentTransactionId}/{jobId}")]
        public async Task<IActionResult> GetPaymentTransactionJobById(string paymentTransactionId, string jobId)
        {
            var pmtVm = await svc.GetPaymentTransactionById("global", paymentTransactionId);
            if (pmtVm.Status != "OK")
            {
                return Ok(pmtVm);
            }

            var pmt = pmtVm.PaymentTransaction!;
            var result = _jobService.GetJobById(pmt.OrgId!, jobId);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPendingPayInRequestsForPaymentTx")]
        public async Task<IActionResult> GetPendingPayInRequestsForPaymentTx(VMPaymentRequest request)
        {
            request.Direction = "PayIn";
            request.Status = "Pending";
            request.FromDate = DateTime.UtcNow.AddHours(-1);
            var paymentRequests = await svc.GetPaymentRequestsForPaymentTx("global", request);

            return Ok(paymentRequests);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInTransactionCount")]
        public async Task<IActionResult> GetPayInTransactionCount([FromBody] VMPaymentTransaction request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentTransactionCount("global", request);

            return Ok(result);
        }
    }
}
