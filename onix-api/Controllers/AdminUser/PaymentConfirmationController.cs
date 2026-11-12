using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    // Anonymous endpoint, called via merchant's own webhook config — auths itself against PP-API-KEY header instead of JWT
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class PaymentConfirmationController : ControllerBase
    {
        private const string ApiKeyHeaderName = "PP-API-KEY";

        private readonly IConfigurationService _configSvc;
        private readonly IAccountDocService _accountDocSvc;

        [ExcludeFromCodeCoverage]
        public PaymentConfirmationController(IConfigurationService configService, IAccountDocService accountDocService)
        {
            _configSvc = configService;
            _accountDocSvc = accountDocService;
        }

        // Real payload is a raw MJob (Parameters[].Name/.Value), not flat {Ref1,Ref2,Ref3} — supports both shapes
        private static string? GetParam(JsonElement root, string jobParamName, string flatName)
        {
            if (root.TryGetProperty("Parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in parameters.EnumerateArray())
                {
                    if (p.TryGetProperty("Name", out var n) && string.Equals(n.GetString(), jobParamName, StringComparison.OrdinalIgnoreCase))
                    {
                        return p.TryGetProperty("Value", out var v) ? v.GetString() : null;
                    }
                }
                return null;
            }

            return root.TryGetProperty(flatName, out var flat) ? flat.GetString() : null;
        }

        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpPost]
        [Route("org/global/action/PleasePaymentConfirmation")]
        public async Task<IActionResult> PleasePaymentConfirmation([FromBody] JsonElement request)
        {
            var apiKeyHeader = Request.Headers.TryGetValue(ApiKeyHeaderName, out var h) ? h.ToString() : "";
            if (string.IsNullOrEmpty(apiKeyHeader))
            {
                return Unauthorized(new { Status = "UNAUTHORIZED", Description = $"Missing {ApiKeyHeaderName} header" });
            }

            var expectedKey = await _configSvc.GetSystemPaymentConfirmationKeyCached("global");
            if (string.IsNullOrEmpty(expectedKey) || apiKeyHeader != expectedKey)
            {
                return Unauthorized(new { Status = "UNAUTHORIZED", Description = $"Invalid {ApiKeyHeaderName}" });
            }

            var ref1 = GetParam(request, "PMR_REF_ID1", "Ref1");
            var ref2 = GetParam(request, "PMR_REF_ID2", "Ref2");
            var eventType = GetParam(request, "EVENT_TYPE", "EventType")
                ?? (request.TryGetProperty("Type", out var t) ? t.GetString() : null);

            // Ref2 identifies a point-wallet-topup call — see PointTopUpService.CreatePointTopUp
            if (!string.IsNullOrEmpty(ref1) && string.Equals(ref2, "Point Purchase", StringComparison.OrdinalIgnoreCase))
            {
                var isRejected = eventType?.Contains("Reject", StringComparison.OrdinalIgnoreCase) == true;
                if (isRejected)
                {
                    // Failed/expired payin request must reject the linked AccountDoc too, or it stays Pending forever
                    var reason = GetParam(request, "STATUS_REASON", "Reason");
                    var rejectResult = await _accountDocSvc.RejectAccountDocByIdAdmin(ref1, string.IsNullOrWhiteSpace(reason) ? "Payin request expired or rejected" : reason);
                    return Ok(rejectResult);
                }

                // Approve side-effect already handles the point wallet topup (ProcessPointTopUpSideEffect)
                var result = await _accountDocSvc.ApproveAccountDocByIdAdmin(ref1);
                return Ok(result);
            }

            // Other payment types can add their own Ref2 branch here later
            return Ok(new { Status = "UNKNOWN_REF_TYPE", Description = $"No handler for Ref2=[{ref2}]" });
        }
    }
}
