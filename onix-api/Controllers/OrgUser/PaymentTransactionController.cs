using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PaymentTransactionController : ControllerBase
    {
        private readonly IPaymentTransactionService _paymentTransactionSvc;
        private readonly IJobService _jobService;

        [ExcludeFromCodeCoverage]
        public PaymentTransactionController(IPaymentTransactionService paymentTransactionSvc, IJobService jobService)
        {
            _paymentTransactionSvc = paymentTransactionSvc;
            _jobService = jobService;
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

            var result = await _paymentTransactionSvc.GetPaymentTransactions(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentTransactionCount")]
        public async Task<IActionResult> GetPaymentTransactionCount(string orgId, [FromBody] VMPaymentTransaction param)
        {
            var result = await _paymentTransactionSvc.GetPaymentTransactionCount(orgId, param);
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
