using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminPaymentDocumentController : ControllerBase
    {
        private readonly IPaymentDocumentService svc;
        private readonly IMerchantService _merSvc;

        [ExcludeFromCodeCoverage]
        public AdminPaymentDocumentController(IPaymentDocumentService service, IMerchantService merchantService)
        {
            svc = service;
            _merSvc = merchantService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPayInSlipUploadPresignedUrl/{merchantId}")]
        public async Task<IActionResult> GetPayInSlipUploadPresignedUrl(string merchantId, [FromBody] VMUploadDocument request)
        {
            var mVMerchant = await _merSvc.GetMerchantById("notused", merchantId);
            if (mVMerchant.Status != "OK")
            {
                return Ok(mVMerchant);
            }

            var mc = mVMerchant.Merchant!;

            var result = await svc.GetPayInSlipUploadPresignedUrl(mc.OrgId!, mc, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }


        [HttpPost]
        [Route("org/global/action/UpdatePayInDocumentById/{paymentDocId}")]
        public async Task<IActionResult> UpdatePayInDocumentById(string paymentDocId, [FromBody] MPaymentDocument request)
        {
            var result = await svc.UpdatePaymentDocumentById("global", paymentDocId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/ApprovePayInDocumentById/{paymentDocId}")]
        public async Task<IActionResult> ApprovePayInDocumentById(string paymentDocId, [FromBody] MPaymentDocument request)
        {
            var result = await svc.ApprovePaymentDocumentById_V2("global", paymentDocId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/RejectPayInDocumentById/{paymentDocId}")]
        public async Task<IActionResult> RejectPayInDocumentById(string paymentDocId, [FromBody] MPaymentDocument request)
        {
            var result = await svc.RejectPaymentDocumentById("global", paymentDocId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/AddPayInDocument/{merchantId}")]
        public async Task<IActionResult> AddPayInDocument(string merchantId, [FromBody] MPaymentDocument request)
        {
            var mVMerchant = await _merSvc.GetMerchantById("notused", merchantId);
            if (mVMerchant.Status != "OK")
            {
                return Ok(mVMerchant);
            }

            var mc = mVMerchant.Merchant!;

            request.Direction = "PayIn";
            request.MerchantId = merchantId;
            request.Currency = "THB";
            request.Status = "Pending";
            request.DocumentType = "PayInSlip";
            var result = await svc.AddPaymentDocument(mc.OrgId!, request);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInDocuments")]
        public async Task<IActionResult> GetPayInDocuments([FromBody] VMPaymentDocument request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentDocuments("global", request);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentDocumentById/{paymentDocumentId}")]
        public async Task<IActionResult> GetPaymentDocumentById(string paymentDocumentId)
        {
            var result = await svc.GetPaymentDocumentById("global", paymentDocumentId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInDocumentCount")]
        public async Task<IActionResult> GetPayInDocumentCount([FromBody] VMPaymentDocument request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentDocumentCount("global", request);

            return Ok(result);
        }
    }
}
