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

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SubmitKtbPaymentTxNotification/{bankAccountId}")]
        public IActionResult SubmitKtbPaymentTxNotification(string bankAccountId, [FromBody] MPaymentNotiKtb request)
        {
            //ใส่เป็น place holder ไว้ก่อน สำหรับธนาคาคาร KTB
            return Ok("");
        }

        [HttpPost]
        [Route("org/global/action/GetPayInTransactions")]
        public async Task<IActionResult> GetPayInTransactions([FromBody] VMPaymentTransaction request)
        {
            request.Direction = "PayIn";
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
