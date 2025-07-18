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
    public class SupplierController : ControllerBase
    {
        private readonly IEntityService svc;

        [ExcludeFromCodeCoverage]
        public SupplierController(IEntityService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddSupplier")]
        public MVEntity? AddSupplier(string id, [FromBody] MEntity request)
        {
            request.EntityType = 2;
            var result = svc.AddEntity(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteSupplierById/{entityId}")]
        public IActionResult DeleteSupplierById(string id, string entityId)
        {
            var result = svc.DeleteEntityById(id, entityId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteSupplierCascadeById/{entityId}")]
        public IActionResult DeleteSupplierCascadeById(string id, string entityId)
        {
            //TODO : Change the implementiation here...
            var result = svc.DeleteEntityById(id, entityId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateSupplierById/{entityId}")]
        public IActionResult UpdateSupplierById(string id, string entityId, [FromBody] MEntity request)
        {
            var result = svc.UpdateEntityById(id, entityId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetSupplierById/{entityId}")]
        public MEntity GetSupplierById(string id, string entityId)
        {
            var result = svc.GetEntityById(id, entityId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetSuppliers")]
        public IActionResult GetSuppliers(string id, [FromBody] VMEntity param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            param.EntityType = 2;
            var result = svc.GetEntities(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetSupplierCount")]
        public IActionResult GetSupplierCount(string id, [FromBody] VMEntity param)
        {
            param.EntityType = 2;
            var result = svc.GetEntityCount(id, param);
            return Ok(result);
        }
    }
}
