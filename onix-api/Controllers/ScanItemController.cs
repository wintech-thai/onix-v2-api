using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ScanItemController : ControllerBase
    {
        private readonly IScanItemService svc;

        [ExcludeFromCodeCoverage]
        public ScanItemController(IScanItemService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AttachScanItemToProduct/{scanItemId}/{productId}")]
        public IActionResult AttachScanItemToProduct(string id, string scanItemId, string productId)
        {
            var result = svc.AttachScanItemToProduct(id, scanItemId, productId);
            return Ok(result);
        }
    }
}
