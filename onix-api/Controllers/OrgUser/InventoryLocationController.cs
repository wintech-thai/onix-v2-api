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
    public class InventoryLocationController : ControllerBase
    {
        private readonly IInventoryLocationService svc;

        [ExcludeFromCodeCoverage]
        public InventoryLocationController(IInventoryLocationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddInventoryLocation")]
        public MVInventoryLocation? AddInventoryLocation(string id, [FromBody] MInventoryLocation request)
        {
            var result = svc.AddInventoryLocation(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteInventoryLocationById/{inventoryLocationId}")]
        public IActionResult DeleteInventoryLocationById(string id, string inventoryLocationId)
        {
            var result = svc.DeleteInventoryLocationById(id, inventoryLocationId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateInventoryLocationById/{inventoryLocationId}")]
        public IActionResult UpdateInventoryLocationById(string id, string inventoryLocationId, [FromBody] MInventoryLocation request)
        {
            var result = svc.UpdateInventoryLocationById(id, inventoryLocationId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInventoryLocationById/{inventoryLocationId}")]
        public MInventoryLocation GetInventoryLocationById(string id, string inventoryLocationId)
        {
            var result = svc.GetInventoryLocationById(id, inventoryLocationId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryLocations")]
        public IActionResult GetInventoryLocations(string id, [FromBody] VMInventoryLocation param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetInventoryLocations(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetInventoryLocationCount")]
        public IActionResult GetInventoryLocationCount(string id, [FromBody] VMInventoryLocation param)
        {
            var result = svc.GetInventoryLocationCount(id, param);
            return Ok(result);
        }
    }
}
