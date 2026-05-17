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
    public class AdminPaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService svc;
        private readonly IMerchantService _merchantSvc;

        [ExcludeFromCodeCoverage]
        public AdminPaymentRequestController(IPaymentRequestService service, IMerchantService merchantService)
        {
            svc = service;
            _merchantSvc = merchantService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SubmitPaymentRequestByMerchantId/{merchantId}")]
        public async Task<IActionResult> SubmitPaymentRequestByMerchantId(string merchantId, [FromBody] MPaymentRequest request)
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
            var result = await svc.AddPaymentRequestPayIn(mc.OrgId, request, mc);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInRequests")]
        public async Task<IActionResult> GetPayInRequests([FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentRequests("global", request);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentRequestById/{paymentRequestId}")]
        public async Task<IActionResult> GetPaymentRequestById(string paymentRequestId)
        {
            var result = await svc.GetPaymentRequestById("global", paymentRequestId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetPayInRequestCount")]
        public async Task<IActionResult> GetAgentCount([FromBody] VMPaymentRequest request)
        {
            var result = await svc.GetPaymentRequestCount("global", request);
            return Ok(result);
        }
    }
}
