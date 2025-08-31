using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;

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
            var result = svc.VerifyScanItem(id, serial, pin);

            var url = cfg["ScanItem:RedirectUrl"]!;

            //TODO : Redirect to https://aldamex.com/scan/data=base64(encrypt(result))
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
