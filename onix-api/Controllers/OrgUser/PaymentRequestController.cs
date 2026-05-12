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

        [ExcludeFromCodeCoverage]
        public PaymentRequestController(IPaymentRequestService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SubmitPaymentRequest")]
        public async Task<IActionResult> SubmitPayInRequest(string id, [FromBody] MPaymentRequest request)
        {
            var result = await svc.AddPaymentRequestPayIn(id, request);
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
