using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService _paymentRequestSvc;

        [ExcludeFromCodeCoverage]
        public PaymentRequestController(IPaymentRequestService paymentRequestSvc)
        {
            _paymentRequestSvc = paymentRequestSvc;
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
    }
}
