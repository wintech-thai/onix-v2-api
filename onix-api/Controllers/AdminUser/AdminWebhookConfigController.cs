using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminWebhookConfigController : ControllerBase
    {
        private readonly IWebhookConfigService svc;

        [ExcludeFromCodeCoverage]
        public AdminWebhookConfigController(IWebhookConfigService service, IMerchantService merchantSvc)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddMerchantWebhookConfig/{orgId}/{merchantId}")]
        public async Task<IActionResult> AddMerchantWebhookConfig(string orgId, string merchantId, [FromBody] MWebhookConfig request)
        {
            var result = await svc.AddMerchantWebhookConfig(orgId, merchantId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetWebhookConfigById/{orgId}/{webhookConfigId}")]
        public async Task<IActionResult> GetWebhookConfigById(string orgId, string webhookConfigId)
        {
            var result = await svc.GetWebhookConfigById(orgId, webhookConfigId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteWebhookById/{orgId}/{webhookConfigId}")]
        public async Task<IActionResult> DeleteWebhookById(string orgId, string webhookConfigId)
        {
            var result = await svc.DeleteWebhookById(orgId, webhookConfigId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetWebhookConfigsByMerchantId/{orgId}/{merchantId}")]
        public async Task<IActionResult> GetWebhookConfigsByMerchantId(string orgId, string merchantId)
        {
            var result = await svc.GetWebhookConfigsByMerchantId(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetWebhookConfigsCountByMerchantId/{orgId}/{merchantId}")]
        public async Task<IActionResult> GetWebhookConfigsCountByMerchantId(string orgId, string merchantId)
        {
            var result = await svc.GetWebhookConfigsCountByMerchantId(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateWebhookConfigById/{orgId}/{webhookConfigId}")]
        public async Task<IActionResult> UpdateWebhookConfigById(string orgId, string webhookConfigId, [FromBody] MWebhookConfig request)
        {
            var result = await svc.UpdateWebhookConfigById(orgId, webhookConfigId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableWebhookConfigById/{orgId}/{webhookConfigId}")]
        public async Task<IActionResult> EnableWebhookConfigById(string orgId, string webhookConfigId)
        {
            var result = await svc.UpdateWebhookConfigStatusById(orgId, webhookConfigId, true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableWebhookConfigById/{orgId}/{webhookConfigId}")]
        public async Task<IActionResult> DisableWebhookConfigById(string orgId, string webhookConfigId)
        {
            var result = await svc.UpdateWebhookConfigStatusById(orgId, webhookConfigId, false);
            return Ok(result);
        }
    }
}
