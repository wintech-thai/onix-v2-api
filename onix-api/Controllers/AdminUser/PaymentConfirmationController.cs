using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // x093 5 : endpoint นี้ anonymous ทั้งหมด (ไม่มี JWT) — Ruby script ภายนอกเป็นคนยิงเข้ามาตอนมีเงินเข้าบัญชีจริง
    // ต้อง authen เองผ่าน header PP-API-KEY เทียบกับ SystemPaymentConfirmationKey (ConfigurationService.GetSystemPaymentConfirmationKeyCached)
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

        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpPost]
        [Route("org/global/action/PleasePaymentConfirmation")]
        public async Task<IActionResult> PleasePaymentConfirmation([FromBody] VMPaymentConfirmation request)
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

            // x093 5biii : เช็คว่านี่คือการเรียกจากการเติมเงิน point wallet จาก Ref2 (ดู PointTopUpService.CreatePointTopUp)
            if (!string.IsNullOrEmpty(request.Ref1) && string.Equals(request.Ref2, "Point Purchase", StringComparison.OrdinalIgnoreCase))
            {
                // x093 5b.i/ii : approve ตัว AccountDoc (PurchaseCash/PointTopUp) เลย — ฝั่ง approve เดิมจัดการ topup point wallet ให้อยู่แล้ว (ProcessPointTopUpSideEffect)
                var result = await _accountDocSvc.ApproveAccountDocByIdAdmin(request.Ref1);
                return Ok(result);
            }

            // x093 5biv : ในอนาคต endpoint นี้รองรับการชำระเงินแบบอื่นที่ไม่ใช่เติมเงิน point wallet ได้ โดยเพิ่ม branch เช็ค Ref2 แบบอื่นตรงนี้
            return Ok(new { Status = "UNKNOWN_REF_TYPE", Description = $"No handler for Ref2=[{request.Ref2}]" });
        }
    }
}
