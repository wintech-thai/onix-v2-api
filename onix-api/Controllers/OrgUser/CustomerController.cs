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
    public class CustomerController : ControllerBase
    {
        private readonly IEntityService svc;

        [ExcludeFromCodeCoverage]
        public CustomerController(IEntityService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddCustomer")]
        public MVEntity? AddCustomer(string id, [FromBody] MEntity request)
        {
            request.EntityType = 1;
            var result = svc.AddEntity(id, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteCustomerById/{entityId}")]
        public IActionResult DeleteCustomerById(string id, string entityId)
        {
            var result = svc.DeleteEntityById(id, entityId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteCustomerCascadeById/{entityId}")]
        public IActionResult DeleteCustomerCascadeById(string id, string entityId)
        {
            //TODO : Change the implementiation here...
            var result = svc.DeleteEntityById(id, entityId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateCustomerById/{entityId}")]
        public IActionResult UpdateCustomerById(string id, string entityId, [FromBody] MEntity request)
        {
            request.EntityType = 1;
            var result = svc.UpdateEntityById(id, entityId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateCustomerEmailById/{entityId}/{email}")]
        public IActionResult UpdateCustomerEmailById(string id, string entityId, string email)
        {
            var result = svc.UpdateEntityEmailById(id, entityId, email, false);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateCustomerEmailWithConfirmationById/{entityId}/{email}")]
        public IActionResult UpdateCustomerEmailWithConfirmationById(string id, string entityId, string email)
        {
            var result = svc.UpdateEntityEmailById(id, entityId, email, true);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetCustomerById/{entityId}")]
        public MEntity GetCustomerById(string id, string entityId)
        {
            var result = svc.GetEntityById(id, entityId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetCustomers")]
        public IActionResult GetCustomers(string id, [FromBody] VMEntity param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            param.EntityType = 1;
            var result = svc.GetEntities(id, param);
            
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetCustomerCount")]
        public IActionResult GetCustomerCount(string id, [FromBody] VMEntity param)
        {
            param.EntityType = 1;
            var result = svc.GetEntityCount(id, param);
            return Ok(result);
        }
    }
}
