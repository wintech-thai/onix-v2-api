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
        private readonly IPaymentRequestService svc;
        private readonly IMerchantService _merchantSvc;

        [ExcludeFromCodeCoverage]
        public AdminPaymentTxController(IPaymentRequestService service, IMerchantService merchantService)
        {
            svc = service;
            _merchantSvc = merchantService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SubmitLinePaymentTxNotification/{bankAccountId}")] 
        public IActionResult SubmitLinePaymentTxNotification(string bankAccountId, [FromBody] MPaymentNotiLine request)
        {
            return Ok("");
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SubmitLinePaymentTxNotification/{bankAccountId}")] 
        public IActionResult SubmitScbPaymentTxNotification(string bankAccountId, [FromBody] MPaymentNotiScb request)
        {
            //ใส่เป็น place holder ไว้ก่อน สำหรับธนาคาคาร SCB
            return Ok("");
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
