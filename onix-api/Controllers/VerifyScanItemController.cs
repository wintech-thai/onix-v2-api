using Serilog;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;
using Its.Onix.Api.Utils;
using System.Text;
using System.Web;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    public class VerifyScanItemController : ControllerBase
    {
        private readonly IScanItemService svc;
        private readonly IRedisHelper _redis;
        private readonly IScanItemActionService _scanItemActionService;

        public VerifyScanItemController(IScanItemService service,
            IScanItemActionService scanItemActionService,
            IRedisHelper redis)
        {
            svc = service;
            _redis = redis;
            _scanItemActionService = scanItemActionService;
        }

        private string CreateUrlWithOTP(string orgId, string scanUrl, string keyword, string apiName, string serial, string pin)
        {
            var url = scanUrl.Replace(keyword, apiName);
            var otp = ServiceUtils.CreateOTP(6);

            var otpObj = new MOtp()
            {
                Otp = otp,
                Id = "",
            };

            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, apiName);
            _ = _redis.SetObjectAsync($"{cacheKey}:{serial}:{pin}", otpObj, TimeSpan.FromMinutes(30));

            url = $"{url}/{otp}";
            //Console.WriteLine($"===== [{url}] =====");

            return url;
        }

        [HttpGet]
        [Route("org/{id}/Verify/{serial}/{pin}")]
        public IActionResult? Verify(string id, string serial, string pin)
        {
            var cacheKey = CacheHelper.CreateScanItemActionKey(id);
            var t = _redis.GetObjectAsync<MScanItemAction>(cacheKey);
            var scanItemAction = t.Result;

            if (scanItemAction == null)
            {
                Log.Information($"Loading scan-item action from cache with key [{cacheKey}]");
                var m = _scanItemActionService!.GetScanItemAction(id);

                if (m == null)
                {
                    Response.Headers.Append("CUST_STATUS", "NO_SCAN_ITEM_ACTION");
                    return BadRequest(new { error = "No default scan-item action is set!!!" });
                }

                _ = _redis.SetObjectAsync(cacheKey, m, TimeSpan.FromMinutes(10));
                scanItemAction = m;
            }

            var baseUrl = scanItemAction!.RedirectUrl;
            var key = scanItemAction!.EncryptionKey;
            var iv = scanItemAction!.EncryptionIV;
            //Console.WriteLine($"@@@@@@@@@@@@@@@@2 [{key}] [{iv}]");
            var result = svc.VerifyScanItem(id, serial, pin);
            result.ThemeVerify = string.IsNullOrWhiteSpace(scanItemAction.ThemeVerify) ? "default" : scanItemAction.ThemeVerify;

            if (scanItemAction.RegisteredAwareFlag == "NO")
            {
                //เป็นตัวบอกว่าจะไม่ให้ความสำคัญกับ ALREADY_REGISTERED
                if (result.Status == "ALREADY_REGISTERED")
                {
                    result.Status = "SUCCESS";
                }
            }

            if (result.ScanItem != null)
            {
                var scanUrl = result.ScanItem!.Url!;
                result.GetProductUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "GetProduct", serial, pin);
                result.GetCustomerUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "GetCustomer", serial, pin);
                result.RegisterCustomerUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "RegisterCustomer", serial, pin);
                result.RequestOtpViaEmailUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "GetOtpViaEmail", serial, pin);
            }

            var jsonString = JsonSerializer.Serialize(result);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);

            var encryptedB64 = EncryptionUtils.Encrypt(jsonString, key!, iv!);
            //var decryptText = EncryptionUtils.Decrypt(encryptedB64, key, iv);

            var urlSafe = HttpUtility.UrlEncode(encryptedB64);
            var url = $"{baseUrl}?org={id}&theme={result.ThemeVerify}&data={urlSafe}";

            //Console.WriteLine($"DEBUG - Encrypted Text (B64) : {encryptedB64}");
            //Console.WriteLine($"DEBUG - Decrypted Text : {decryptText}");
            //Console.WriteLine($"DEBUG - URL Safe : {urlSafe}");

            Response.Headers.Append("CUST_STATUS", result.Status);
            return Redirect(url);
        }

        [HttpGet]
        [Route("org/{id}/VerifyScanItem/{serial}/{pin}")]
        public MVScanItemResult? VerifyScanItem(string id, string serial, string pin)
        {
            var result = svc.VerifyScanItem(id, serial, pin);

            if (result.ScanItem != null)
            {
                var scanUrl = result.ScanItem!.Url!;
                result.GetProductUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "GetProduct", serial, pin);
                result.GetCustomerUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "GetCustomer", serial, pin);
                result.RegisterCustomerUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "RegisterCustomer", serial, pin);
                result.RequestOtpViaEmailUrl = CreateUrlWithOTP(id, scanUrl, "Verify", "SendOtpViaEmail", serial, pin);
            }

            Response.Headers.Append("CUST_STATUS", result.Status);
            return result;
        }

        [HttpGet]
        [Route("org/{id}/GetProduct/{serial}/{pin}/{otp}")]
        public MVItem? GetProduct(string id, string serial, string pin, string otp)
        {
            var result = svc.GetScanItemProduct(id, serial, pin, otp);
            Response.Headers.Append("CUST_STATUS", result.Status);
            return result;
        }

        [HttpGet]
        [Route("org/{id}/GetCustomer/{serial}/{pin}/{otp}")]
        public MVEntityRestrictedInfo? GetCustomer(string id, string serial, string pin, string otp)
        {
            var result = svc.GetScanItemCustomer(id, serial, pin, otp);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var r = ServiceUtils.MaskingEntity(result);
            return r;
        }

        [HttpPost]
        [Route("org/{id}/RegisterCustomer/{serial}/{pin}/{otp}")]
        public MVEntityRestrictedInfo? RegisterCustomer(string id, string serial, string pin, string otp, [FromBody] MCustomerRegister request)
        {
            var result = svc.RegisterCustomer(id, serial, pin, otp, request);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var r = ServiceUtils.MaskingEntity(result);
            return r;
        }

        [HttpGet]
        [Route("org/{id}/GetOtpViaEmail/{serial}/{pin}/{otp}/{email}")]
        public MVOtp? GetOtpViaEmail(string id, string serial, string pin, string otp, string email)
        {
            var result = svc.GetOtpViaEmail(id, serial, pin, otp, email);
            Response.Headers.Append("CUST_STATUS", result.Status);

            //Console.WriteLine($"==== Sent this OTP [{result.OTP}] to [{email}] ====");

            return result;
        }
    }
}
