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
    public class PaymentDocumentController : ControllerBase
    {
        private readonly IPaymentDocumentService _paymentDocumentSvc;
        private readonly IMerchantService _merchantSvc;

        [ExcludeFromCodeCoverage]
        public PaymentDocumentController(IPaymentDocumentService paymentDocumentSvc, IMerchantService merchantSvc)
        {
            _paymentDocumentSvc = paymentDocumentSvc;
            _merchantSvc = merchantSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentDocuments")]
        public async Task<IActionResult> GetPaymentDocuments(string orgId, [FromBody] VMPaymentDocument param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await _paymentDocumentSvc.GetPaymentDocuments(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPaymentDocumentCount")]
        public async Task<IActionResult> GetPaymentDocumentCount(string orgId, [FromBody] VMPaymentDocument param)
        {
            var result = await _paymentDocumentSvc.GetPaymentDocumentCount(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetPaymentDocumentById/{paymentDocumentId}")]
        public async Task<IActionResult> GetPaymentDocumentById(string orgId, string paymentDocumentId)
        {
            var result = await _paymentDocumentSvc.GetPaymentDocumentById(orgId, paymentDocumentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetPresignedUrl")]
        public async Task<IActionResult> GetPresignedUrl(string orgId, [FromBody] VMUploadDocument request)
        {
            var param = new VMMerchant { Limit = 1, Offset = 0 };
            var merchants = await _merchantSvc.GetMerchants(orgId, param);

            if (merchants == null || merchants.Count == 0)
            {
                return Ok(new { Status = "Error", Description = "No merchant found for this org" });
            }

            var merchant = merchants[0];
            var result = await _paymentDocumentSvc.GetPayInSlipUploadPresignedUrl(orgId, merchant, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/AddPaymentDocument")]
        public async Task<IActionResult> AddPaymentDocument(string orgId, [FromBody] MPaymentDocument request)
        {
            var result = await _paymentDocumentSvc.AddPaymentDocument(orgId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/UpdatePaymentDocumentById/{paymentDocumentId}")]
        public async Task<IActionResult> UpdatePaymentDocumentById(string orgId, string paymentDocumentId, [FromBody] MPaymentDocument request)
        {
            var result = await _paymentDocumentSvc.UpdatePaymentDocumentById(orgId, paymentDocumentId, request);
            return Ok(result);
        }
    }
}
