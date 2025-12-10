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
        public async Task<IActionResult> AttachScanItemToProduct(string id, string scanItemId, string productId)
        {
            var result = await svc.AttachScanItemToProduct(id, scanItemId, productId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AttachScanItemToCustomer/{scanItemId}/{customerId}")]
        public async Task<IActionResult> AttachScanItemToCustomer(string id, string scanItemId, string customerId)
        {
            var result = await svc.AttachScanItemToCustomer(id, scanItemId, customerId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DetachScanItemFromCustomer/{scanItemId}")]
        public async Task<IActionResult> DetachScanItemFromCustomer(string id, string scanItemId)
        {
            var result = await svc.DetachScanItemFromCustomer(id, scanItemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DetachScanItemFromProduct/{scanItemId}")]
        public async Task<IActionResult> DetachScanItemFromProduct(string id, string scanItemId)
        {
            var result = await svc.DetachScanItemFromProduct(id, scanItemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetScanItemById/{scanItemId}")]
        public async Task<IActionResult> GetItemById(string id, string scanItemId)
        {
            var result = await svc.GetScanItemByIdV2(id, scanItemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddScanItem")]
        public async Task<IActionResult> AddScanItem(string id, [FromBody] MScanItem request)
        {
            var result = await svc.AddScanItemV2(id, request);

            Response.Headers.Append("CUST_STATUS", result.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteScanItemById/{scanItemId}")]
        public async Task<IActionResult> DeleteScanItemById(string id, string scanItemId)
        {
            var result = await svc.DeleteScanItemByIdV2(id, scanItemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/UnVerifyScanItemById/{scanItemId}")]
        public async Task<IActionResult> UnVerifyScanItemById(string id, string scanItemId)
        {
            var result = await svc.UnVerifyScanItemByIdV2(id, scanItemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetScanItemCount")]
        public async Task<IActionResult> GetScanItemCount(string id, [FromBody] VMScanItem param)
        {
            var result = await svc.GetScanItemCountV2(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetScanItems")]
        public async Task<IActionResult> GetScanItems(string id, [FromBody] VMScanItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }
//Console.WriteLine($"====== DEBUG ==== limit=[{param.Limit}], offset=[{param.Offset}]");
            var result = await svc.GetScanItemsV2(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetScanItemUrlDryRunById/{scanItemId}")]
        public async Task<IActionResult> GetScanItemUrlDryRunById(string id, string scanItemId)
        {
            var result = await svc.GetScanItemUrlDryRunById(id, scanItemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
    }
}
