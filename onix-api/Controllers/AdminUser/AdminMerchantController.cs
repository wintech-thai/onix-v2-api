using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminMerchantController : ControllerBase
    {
        private readonly IMerchantService svc;

        [ExcludeFromCodeCoverage]
        public AdminMerchantController(IMerchantService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetMerchantById/{merchantId}")]
        public async Task<IActionResult> GetMerchantById(string merchantId)
        {
            var result = await svc.GetMerchantById("notused", merchantId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetMerchants")]
        public async Task<IActionResult> GetMerchants([FromBody] VMMerchant param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetMerchants("notused", param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetMerchantCount")]
        public async Task<IActionResult> GetMerchantCount([FromBody] VMMerchant param)
        {
            var result = await svc.GetMerchantCount("notused", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateMerchantById/{merchantId}")]
        public async Task<IActionResult> UpdateMerchantById(string merchantId, [FromBody] MMerchant request)
        {
            var result = await svc.UpdateMerchantById("notused", merchantId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableMerchantById/{merchantId}")]
        public async Task<IActionResult> EnableMerchantById(string merchantId)
        {
            var result = await svc.UpdateMerchantStatusById(merchantId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableMerchantById/{merchantId}")]
        public async Task<IActionResult> DisableMerchantById(string merchantId)
        {
            var result = await svc.UpdateMerchantStatusById(merchantId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
    }
}
