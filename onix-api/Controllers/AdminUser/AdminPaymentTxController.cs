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

        [ExcludeFromCodeCoverage]
        public AdminPaymentTxController(IPaymentTransactionService service)
        {
            svc = service;
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
