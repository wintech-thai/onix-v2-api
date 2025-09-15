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
        private readonly IConfiguration cfg;
        private readonly RedisHelper _redis;
        private readonly IScanItemActionService _scanItemActionService;

        [ExcludeFromCodeCoverage]
        public VerifyScanItemController(IScanItemService service,
            IScanItemActionService scanItemActionService,
            RedisHelper redis,
            IConfiguration config)
        {
            svc = service;
            cfg = config;
            _redis = redis;
            _scanItemActionService = scanItemActionService;
        }

        [ExcludeFromCodeCoverage]
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

                _ = _redis.SetObjectAsync(cacheKey, m, TimeSpan.FromMinutes(300));
                scanItemAction = m;
            }

            var baseUrl = scanItemAction!.RedirectUrl;
            var key = scanItemAction!.EncryptionKey;
            var iv = scanItemAction!.EncryptionIV;

            var result = svc.VerifyScanItem(id, serial, pin);
            var jsonString = JsonSerializer.Serialize(result);

            if (result.ScanItem != null)
            {
                var scanUrl = result.ScanItem!.Url!;
                result.GetProductUrl = scanUrl.Replace("Verify", "GetProduct");
            }

            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);
            
            var encryptedB64 = EncryptionUtils.Encrypt(jsonString, key!, iv!);
            //var decryptText = EncryptionUtils.Decrypt(encryptedB64, key, iv);

            var urlSafe = HttpUtility.UrlEncode(encryptedB64);
            var url = $"{baseUrl}?data={urlSafe}";

            //Console.WriteLine($"DEBUG - Encrypted Text (B64) : {encryptedB64}");
            //Console.WriteLine($"DEBUG - Decrypted Text : {decryptText}");
            //Console.WriteLine($"DEBUG - URL Safe : {urlSafe}");

            Response.Headers.Append("CUST_STATUS", result.Status);
            return Redirect(url);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/VerifyScanItem/{serial}/{pin}")]
        public MVScanItemResult? VerifyScanItem(string id, string serial, string pin)
        {
            var result = svc.VerifyScanItem(id, serial, pin);
            //result.RedirectUrl = cfg["ScanItem:RedirectUrl"]!;

            if (result.ScanItem != null)
            {
                var scanUrl = result.ScanItem!.Url!;
                result.GetProductUrl = scanUrl.Replace("Verify", "GetProduct");
            }

            Response.Headers.Append("CUST_STATUS", result.Status);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/GetProduct/{serial}/{pin}")]
        public MVItem? GetProduct(string id, string serial, string pin)
        {
            var result = svc.GetScanItemProduct(id, serial, pin);
            Response.Headers.Append("CUST_STATUS", result.Status);
            return result;
        }
    }
}
