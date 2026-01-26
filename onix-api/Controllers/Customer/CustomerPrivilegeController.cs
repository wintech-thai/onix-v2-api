using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/customer-api/[controller]")]
    public class CustomerPrivilegeController : ControllerBase
    {
        private readonly IItemService svc;
        private readonly IVoucherService voucherSvc;

        [ExcludeFromCodeCoverage]
        public CustomerPrivilegeController(IItemService service, IVoucherService voucherService)
        {
            svc = service;
            voucherSvc = voucherService;
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
        [HttpPost]
        [Route("org/{orgId}/action/GetRedeemablePrivileges")]
        public IActionResult GetRedeemablePrivileges(string orgId, [FromBody] VMItem param)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            param.ItemType = 2; //Privilege
            param.Status = "Approve";
            var result = svc.GetItems(orgId, param);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/RedeemPrivilegeById/{privilegeId}")]
        public async Task<IActionResult> RedeemPrivilegeById(string orgId, string privilegeId)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var customerId = validateResult.CustomerId;
            var voucher = new MVoucher()
            {
                CustomerId = customerId,
                PrivilegeId = privilegeId,
            };
            var result = await voucherSvc.AddVoucher(orgId, voucher);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetVouchers")]
        public async Task<IActionResult> GetVouchers(string id, [FromBody] VMVoucher request)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var customerId = validateResult.CustomerId;
            request.CustomerId = customerId;

            var result = await voucherSvc.GetVouchers(id, request);

            var filteredVouchers = result
                .Where(v => v.CustomerId == customerId)
                .ToList();

            return Ok(filteredVouchers);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetVoucherCount")]
        public async Task<IActionResult> GetVoucherCount(string id, [FromBody] VMVoucher request)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var customerId = validateResult.CustomerId;
            request.CustomerId = customerId;

            var result = await voucherSvc.GetVoucherCount(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetVoucherById/{voucherId}")]
        public async Task<IActionResult> GetVoucherById(string id, string voucherId)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var customerId = validateResult.CustomerId;
            var result = await voucherSvc.GetVoucherById(id, voucherId);

            if (result.Voucher != null && result.Voucher.CustomerId != customerId)
            {
                result.Voucher = null;
                result.Status = "UNAUTHORIZED_ACCESS_VOUCHER";
                result.Description = "The voucher does not belong to the authenticated customer.";

                return Ok(result);
            }

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetVoucherVerifyQrUrl/{voucherId}")]
        public async Task<IActionResult> GetVoucherVerifyQrUrl(string id, string voucherId)
        {
            var validateResult = ValidateCustomerIdentity();
            if (string.IsNullOrEmpty(validateResult.CustomerId))
            {
                return validateResult.RequestResult!;
            }

            var customerId = validateResult.CustomerId;
            var result = await voucherSvc.GetVoucherById(id, voucherId);

            if (result.Voucher != null && result.Voucher.CustomerId != customerId)
            {
                result.Voucher = null;
                result.Status = "UNAUTHORIZED_ACCESS_VOUCHER";
                result.Description = "The voucher does not belong to the authenticated customer.";

                return Ok(result);
            }

            result = await voucherSvc.GetVoucherVerifyQrUrl(id, voucherId);
            return Ok(result);
        }
    }
}
