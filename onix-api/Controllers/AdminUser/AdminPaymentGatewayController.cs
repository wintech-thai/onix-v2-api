using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminPaymentGatewayController : ControllerBase
    {
        private readonly IPaymentGatewayService svc;

        [ExcludeFromCodeCoverage]
        public AdminPaymentGatewayController(IPaymentGatewayService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddPaymentGateway")]
        public async Task<IActionResult> AddPaymentGateway([FromBody] MPaymentGateway request)
        {
            var result = await svc.AddPaymentGateway("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeletePaymentGatewayById/{paymentGatewayId}")]
        public async Task<IActionResult> DeletePaymentGatewayById(string paymentGatewayId)
        {
            var result = await svc.DeletePaymentGatewayById("global", paymentGatewayId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPaymentGateways")]
        public async Task<IActionResult> GetPaymentGateways([FromBody] VMPaymentGateway param)
        {
            var result = await svc.GetPaymentGateways("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetPaymentGatewayCount")]
        public async Task<IActionResult> GetPaymentGatewayCount([FromBody] VMPaymentGateway param)
        {
            var result = await svc.GetPaymentGatewayCount("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetPaymentGatewayById/{paymentGatewayId}")]
        public async Task<IActionResult> GetPaymentGatewayById(string paymentGatewayId)
        {
            var result = await svc.GetPaymentGatewayById("global", paymentGatewayId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdatePaymentGatewayById/{paymentGatewayId}")]
        public async Task<IActionResult> UpdatePaymentGatewayById(string paymentGatewayId, [FromBody] MPaymentGateway request)
        {
            var result = await svc.UpdatePaymentGatewayById("global", paymentGatewayId, request);
            return Ok(result);
        }
    }
}
