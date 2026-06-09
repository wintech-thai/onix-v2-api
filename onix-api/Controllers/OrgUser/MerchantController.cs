using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using System.Security.Claims;

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
        private readonly IOrganizationService _orgSvc;

        [ExcludeFromCodeCoverage]
        public MerchantController(
            IMerchantService merchantSvc,
            IWebhookConfigService webhookSvc,
            IPointService pointSvc,
            IOrganizationService orgSvc)
        {
            _merchantSvc = merchantSvc;
            _webhookSvc = webhookSvc;
            _pointSvc = pointSvc;
            _orgSvc = orgSvc;
        }

        private string? GetCurrentOrgId()
        {
            return User.FindFirst(ClaimTypes.GroupSid)?.Value;
        }
        private async Task<string?> GetCurrentMerchantId(string requestedOrgId)
        {
            if (string.IsNullOrEmpty(requestedOrgId))
            {
                return null;
            }

            var param = new VMMerchant { Limit = 100, Offset = 0 };
            var merchants = await _merchantSvc.GetMerchants(requestedOrgId, param);

            return merchants.FirstOrDefault(m => m.OrgId == requestedOrgId)?.Id?.ToString();
        }
        // comment
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyMerchantId")]
        public async Task<IActionResult> GetMyMerchantId(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

            return Ok(new { Status = "OK", MerchantId = merchantId });
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMyMerchantInfo")]
        public async Task<IActionResult> GetMyMerchantInfo(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

            var result = await _merchantSvc.GetMerchantById(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMerchantPaymentEndpoint")]
        public async Task<IActionResult> GetMerchantPaymentEndpoint(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

            var mvMerchant = await _merchantSvc.GetMerchantById(orgId, merchantId);
            if (mvMerchant.Status != "OK")
            {
                return Ok(mvMerchant);
            }

            var mc = mvMerchant.Merchant!;
            var merchantOrgId = mc.OrgId;
            var url = $"https://<PAYMENT-REQUEST-SERVICE>/api/PaymentRequest/org/{merchantOrgId}/action/SubmitPayInRequest/{merchantId}";

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
        [Route("org/{orgId}/action/GetMerchantPayOutEndpoint")]
        public async Task<IActionResult> GetMerchantPayOutEndpoint(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

            var mvMerchant = await _merchantSvc.GetMerchantById(orgId, merchantId);
            if (mvMerchant.Status != "OK")
            {
                return Ok(mvMerchant);
            }

            var mc = mvMerchant.Merchant!;
            var merchantOrgId = mc.OrgId;
            var url = $"https://<PAYMENT-REQUEST-SERVICE>/api/PaymentRequest/org/{merchantOrgId}/action/SubmitPayOutRequest/{merchantId}";

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
        [Route("org/{orgId}/action/GetMerchantWebhooks")]
        public async Task<IActionResult> GetMerchantWebhooks(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

            var result = await _webhookSvc.GetWebhookConfigsByMerchantId(orgId, merchantId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetMerchantWallet")]
        public async Task<IActionResult> GetMerchantWallet(string orgId)
        {
            var merchantId = await GetCurrentMerchantId(orgId);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Ok(new { Status = "Error", Description = "Merchant not found" });
            }

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
