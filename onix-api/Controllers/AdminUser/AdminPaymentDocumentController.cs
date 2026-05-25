using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

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
