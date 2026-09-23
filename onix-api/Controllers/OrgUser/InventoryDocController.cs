using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // Backs StockIn today; StockOut/StockTransfer will reuse the same actions later,
    // switching on VMInventoryDoc.DocumentType / MInventoryDoc.DocumentType.
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
        public IActionResult UpdateInventoryDocStockIn(string id, string inventoryDocId, [FromBody] MInventoryDoc request)
        {
            var result = svc.UpdateInventoryDocStockIn(id, inventoryDocId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/ApproveInventoryDocStockIn/{inventoryDocId}")]
        public IActionResult ApproveInventoryDocStockIn(string id, string inventoryDocId)
        {
            var result = svc.ApproveInventoryDocStockIn(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CancelInventoryDocStockIn/{inventoryDocId}")]
        public IActionResult CancelInventoryDocStockIn(string id, string inventoryDocId)
        {
            var result = svc.CancelInventoryDocStockIn(id, inventoryDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInventoryDocById/{inventoryDocId}")]
        public IActionResult GetInventoryDocById(string id, string inventoryDocId)
        {
            var result = svc.GetInventoryDocById(id, inventoryDocId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryDocs")]
        public IActionResult GetInventoryDocs(string id, [FromBody] VMInventoryDoc param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetInventoryDocs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryDocCount")]
        public IActionResult GetInventoryDocCount(string id, [FromBody] VMInventoryDoc param)
        {
            var result = svc.GetInventoryDocCount(id, param);
            return Ok(result);
        }
    }
}
