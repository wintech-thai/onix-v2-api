using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // Backs StockIn, StockOut, and StockTransfer — each gets its own Add/Update/Approve/Cancel
    // actions (same shape, different DocumentType set by the service layer) but shares the
    // read-side actions (GetInventoryDoc*), which filter by VMInventoryDoc.DocumentType.
    //
    // Fully async end to end (controller/service/repository) — team convention for all API work.
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class InventoryDocController : ControllerBase
    {
        private readonly IInventoryDocService svc;

        [ExcludeFromCodeCoverage]
        public InventoryDocController(IInventoryDocService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddInventoryDocStockIn")]
        public async Task<IActionResult> AddInventoryDocStockIn(string id, [FromBody] MInventoryDoc request)
        {
            var result = await svc.AddInventoryDocStockIn(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateInventoryDocStockIn/{inventoryDocId}")]
        public async Task<IActionResult> UpdateInventoryDocStockIn(string id, string inventoryDocId, [FromBody] MInventoryDoc request)
        {
            var result = await svc.UpdateInventoryDocStockIn(id, inventoryDocId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/ApproveInventoryDocStockIn/{inventoryDocId}")]
        public async Task<IActionResult> ApproveInventoryDocStockIn(string id, string inventoryDocId)
        {
            var result = await svc.ApproveInventoryDocStockIn(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CancelInventoryDocStockIn/{inventoryDocId}")]
        public async Task<IActionResult> CancelInventoryDocStockIn(string id, string inventoryDocId)
        {
            var result = await svc.CancelInventoryDocStockIn(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddInventoryDocStockOut")]
        public async Task<IActionResult> AddInventoryDocStockOut(string id, [FromBody] MInventoryDoc request)
        {
            var result = await svc.AddInventoryDocStockOut(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateInventoryDocStockOut/{inventoryDocId}")]
        public async Task<IActionResult> UpdateInventoryDocStockOut(string id, string inventoryDocId, [FromBody] MInventoryDoc request)
        {
            var result = await svc.UpdateInventoryDocStockOut(id, inventoryDocId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/ApproveInventoryDocStockOut/{inventoryDocId}")]
        public async Task<IActionResult> ApproveInventoryDocStockOut(string id, string inventoryDocId)
        {
            var result = await svc.ApproveInventoryDocStockOut(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CancelInventoryDocStockOut/{inventoryDocId}")]
        public async Task<IActionResult> CancelInventoryDocStockOut(string id, string inventoryDocId)
        {
            var result = await svc.CancelInventoryDocStockOut(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddInventoryDocTransfer")]
        public async Task<IActionResult> AddInventoryDocTransfer(string id, [FromBody] MInventoryDoc request)
        {
            var result = await svc.AddInventoryDocTransfer(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateInventoryDocTransfer/{inventoryDocId}")]
        public async Task<IActionResult> UpdateInventoryDocTransfer(string id, string inventoryDocId, [FromBody] MInventoryDoc request)
        {
            var result = await svc.UpdateInventoryDocTransfer(id, inventoryDocId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/ApproveInventoryDocTransfer/{inventoryDocId}")]
        public async Task<IActionResult> ApproveInventoryDocTransfer(string id, string inventoryDocId)
        {
            var result = await svc.ApproveInventoryDocTransfer(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CancelInventoryDocTransfer/{inventoryDocId}")]
        public async Task<IActionResult> CancelInventoryDocTransfer(string id, string inventoryDocId)
        {
            var result = await svc.CancelInventoryDocTransfer(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInventoryDocById/{inventoryDocId}")]
        public async Task<IActionResult> GetInventoryDocById(string id, string inventoryDocId)
        {
            var result = await svc.GetInventoryDocById(id, inventoryDocId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryDocs")]
        public async Task<IActionResult> GetInventoryDocs(string id, [FromBody] VMInventoryDoc param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetInventoryDocs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryDocCount")]
        public async Task<IActionResult> GetInventoryDocCount(string id, [FromBody] VMInventoryDoc param)
        {
            var result = await svc.GetInventoryDocCount(id, param);
            return Ok(result);
        }
    }
}
