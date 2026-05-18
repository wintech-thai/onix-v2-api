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
    [Route("/api/[controller]")]
    public class PaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService svc;
        private readonly IMerchantService _merchantSvc;

        [ExcludeFromCodeCoverage]
        public PaymentRequestController(IPaymentRequestService service, IMerchantService merchantService)
        {
            svc = service;
            _merchantSvc = merchantService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SubmitPaymentRequest/{merchantId}")]
        public async Task<IActionResult> SubmitPaymentRequest(string id, string merchantId, [FromBody] MPaymentRequest request)
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

            var result = await svc.AddPaymentRequestPayIn(id, request, mc);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetPayInRequests")]
        public async Task<IActionResult> GetPayInRequests(string id, [FromBody] VMPaymentRequest request)
        {
            request.Direction = "PayIn";
            var result = await svc.GetPaymentRequests(id, request);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetPayInRequestCount")]
        public async Task<IActionResult> GetAgentCount(string id, [FromBody] VMPaymentRequest request)
        {
            var result = await svc.GetPaymentRequestCount(id, request);
            return Ok(result);
        }
    }
}
