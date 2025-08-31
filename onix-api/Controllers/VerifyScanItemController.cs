using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;
using Its.Onix.Api.Utils;
using System.Text;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    public class VerifyScanItemController : ControllerBase
    {
        private readonly IScanItemService svc;
        private readonly IConfiguration cfg;

        [ExcludeFromCodeCoverage]
        public VerifyScanItemController(IScanItemService service, IConfiguration config)
        {
            svc = service;
            cfg = config;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/Verify/{serial}/{pin}")]
        public IActionResult? Verify(string id, string serial, string pin)
        {
            var baseUrl = cfg["ScanItem:RedirectUrl"]!;
            var key = cfg["ScanItem:SymmetricKey"]!;
            var iv = cfg["ScanItem:EncryptionIv"]!;

            var result = svc.VerifyScanItem(id, serial, pin);
            var jsonString = JsonSerializer.Serialize(result);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);
            
            var encryptedB64 = EncryptionUtils.Encrypt(jsonString, key, iv);
            var decryptText = EncryptionUtils.Decrypt(encryptedB64, key, iv);
            Console.WriteLine($"DEBUG - Decrypted Text : {decryptText}");

            var url = $"{baseUrl}?data={encryptedB64}";

            return Redirect(url);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/VerifyScanItem/{serial}/{pin}")]
        public MVScanItemResult? VerifyScanItem(string id, string serial, string pin)
        {
            var result = svc.VerifyScanItem(id, serial, pin);
            result.RedirectUrl = cfg["ScanItem:RedirectUrl"]!;

            return result;
        }
    }
}
