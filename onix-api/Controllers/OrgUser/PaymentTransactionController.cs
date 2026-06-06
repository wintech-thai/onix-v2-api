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

        [ExcludeFromCodeCoverage]
        public PaymentTransactionController(IPaymentTransactionService paymentTransactionSvc)
        {
            _paymentTransactionSvc = paymentTransactionSvc;
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
    }
}
