using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/customer-api/[controller]")]
    public class CustomerPointController : ControllerBase
    {
        private readonly IPointService svc;

        [ExcludeFromCodeCoverage]
        public CustomerPointController(IPointService service)
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

            result.CustomerId = customerId.ToString();

            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetWallet")]
        public async Task<IActionResult> GetWallet(string orgId)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var customerId = validateResult.CustomerId;
            var result = await svc.GetWalletByCustomerId(orgId, customerId);

            var verifiedResult = svc.ValidateResponseData(orgId, customerId, result!);
            if (verifiedResult!.Status != "OK")
            {
                //เพื่อป้องกันว่า data ที่ส่งออกไปต้องเป็นของ org และ customer ID ที่ request เข้ามาจริง ๆ
                
                Response.Headers.Append("CUST_STATUS", verifiedResult.Status);
                Response.Headers.Append("CUST_DESC", verifiedResult.Description);
                return Ok(verifiedResult);
            }

            Response.Headers.Append("CUST_STATUS", result!.Status);
            Response.Headers.Append("CUST_DESC", result!.Description);

            return Ok(result);
        }
    }
}
