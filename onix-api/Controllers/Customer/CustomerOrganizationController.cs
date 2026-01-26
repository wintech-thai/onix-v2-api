using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/customer-api/[controller]")]
    public class CustomerOrganizationController : ControllerBase
    {
        private readonly IOrganizationService svc;

        [ExcludeFromCodeCoverage]
        public CustomerOrganizationController(IOrganizationService service)
        {
            svc = service;
        }

        private IdentityValidationResult ValidateCustomerIdentity()
        {
            //TODO : Move this to someware we can reuse later
            var result = new IdentityValidationResult();

            var idTypeObj = Response.HttpContext.Items["Temp-Identity-Type"];
            if (idTypeObj == null)
            {
                var obj = BadRequest("Unable to identify identity type!!!");
                result.RequestResult = obj;

                return result;
            }

            var idType = idTypeObj.ToString();
            if (idType != "JWT")
            {
                var obj = BadRequest("Only allow for JWT identity type!!!");
                result.RequestResult = obj;

                return result;
            }

            var customerId = Response.HttpContext.Items["Temp-Customer-Id"];
            if (customerId == null)
            {
                var obj = BadRequest("Unable to find customer ID !!!");
                result.RequestResult = obj;

                return result;
            }

            if (customerId.ToString()!.Trim() == "")
            {
                var obj = BadRequest("Customer ID is empty !!!");
                result.RequestResult = obj;

                return result;
            }

            result.CustomerId = customerId.ToString();

            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetOrganization")]
        public async Task<IActionResult> GetOrganization(string id)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var result = await svc.GetOrganization(id);
            return Ok(result);
        }
    }
}
