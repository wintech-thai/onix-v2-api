using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantSvc;
        private readonly IWebhookConfigService _webhookSvc;
        private readonly IPointService _pointSvc;

        [ExcludeFromCodeCoverage]
        public MerchantController(
            IMerchantService merchantSvc,
            IWebhookConfigService webhookSvc,
            IPointService pointSvc)
        {
            _merchantSvc = merchantSvc;
            _webhookSvc = webhookSvc;
            _pointSvc = pointSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyMerchantInfo/{merchantId}")]
        public async Task<IActionResult> GetMyMerchantInfo(string orgId, string merchantId)
        {
            var result = await _merchantSvc.GetMerchantById(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMerchantPaymentEndpoint/{merchantId}")]
        public async Task<IActionResult> GetMerchantPaymentEndpoint(string orgId, string merchantId)
        {
            var mvMerchant = await _merchantSvc.GetMerchantById(orgId, merchantId);
            if (mvMerchant.Status != "OK")
            {
                return Ok(mvMerchant);
            }

            var mc = mvMerchant.Merchant!;
            var merchantOrgId = mc.OrgId;
            var url = $"https://<PAYMENT-REQUEST-SERVICE>/api/PaymentRequest/org/{merchantOrgId}/action/SubmitPaymentRequest/{merchantId}";

            var result = new MVEndPoint()
            {
                Status = "OK",
                Description = "Success",
                PaymentRequestUrl = url,
            };

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMerchantWebhooks/{merchantId}")]
        public async Task<IActionResult> GetMerchantWebhooks(string orgId, string merchantId)
        {
            var result = await _webhookSvc.GetWebhookConfigsByMerchantId(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMerchantWallet/{merchantId}")]
        public async Task<IActionResult> GetMerchantWallet(string orgId, string merchantId)
        {
            var result = await _pointSvc.GetWalletByMerchantId(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetMerchantWalletTxs/{walletId}")]
        public async Task<IActionResult> GetMerchantWalletTxs(string orgId, string walletId, [FromBody] VMPointTx request)
        {
            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }

            request.WalletId = walletId;
            var result = await _pointSvc.GetPointTxsByWalletId(orgId, request);
            return Ok(result);
        }
    }
}
