using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class InventoryItemController : ControllerBase
    {
        private readonly IInventoryItemService svc;

        [ExcludeFromCodeCoverage]
        public InventoryItemController(IInventoryItemService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddInventoryItem")]
        public MVInventoryItem? AddInventoryItem(string id, [FromBody] MInventoryItem request)
        {
            var result = svc.AddInventoryItem(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteInventoryItemById/{inventoryItemId}")]
        public IActionResult DeleteInventoryItemById(string id, string inventoryItemId)
        {
            var result = svc.DeleteInventoryItemById(id, inventoryItemId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateInventoryItemById/{inventoryItemId}")]
        public IActionResult UpdateInventoryItemById(string id, string inventoryItemId, [FromBody] MInventoryItem request)
        {
            var result = svc.UpdateInventoryItemById(id, inventoryItemId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInventoryItemById/{inventoryItemId}")]
        public MInventoryItem GetInventoryItemById(string id, string inventoryItemId)
        {
            var result = svc.GetInventoryItemById(id, inventoryItemId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryItems")]
        public IActionResult GetInventoryItems(string id, [FromBody] VMInventoryItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetInventoryItems(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryItemCount")]
        public IActionResult GetInventoryItemCount(string id, [FromBody] VMInventoryItem param)
        {
            var result = svc.GetInventoryItemCount(id, param);
            return Ok(result);
        }
    }
}
