using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

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

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetScanItemById/{scanItemId}")]
        public IActionResult GetItemById(string id, string scanItemId)
        {
            var result = svc.GetScanItemById(id, scanItemId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddScanItem")]
        public IActionResult AddScanItem(string id, [FromBody] MScanItem request)
        {
            var result = svc.AddScanItem(id, request);
            Response.Headers.Append("CUST_STATUS", result.Status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteScanItemById/{scanItemId}")]
        public IActionResult DeleteScanItemById(string id, string scanItemId)
        {
            var result = svc.DeleteScanItemById(id, scanItemId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/UnVerifyScanItemById/{scanItemId}")]
        public IActionResult UnVerifyScanItemById(string id, string scanItemId)
        {
            var result = svc.UnVerifyScanItemById(id, scanItemId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetScanItemCount")]
        public IActionResult GetScanItemCount(string id, [FromBody] VMScanItem param)
        {
            var result = svc.GetScanItemCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetScanItems")]
        public IActionResult GetScanItems(string id, [FromBody] VMScanItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetScanItems(id, param);
            return Ok(result);
        }
    }
}
