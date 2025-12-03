using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService svc;

        [ExcludeFromCodeCoverage]
        public VoucherController(IVoucherService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetVouchers")]
        public async Task<IActionResult> GetVouchers(string id, [FromBody] VMVoucher request)
        {
            var result = await svc.GetVouchers(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetVoucherCount")]
        public async Task<IActionResult> GetVoucherCount(string id, [FromBody] VMVoucher request)
        {
            var result = await svc.GetVoucherCount(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddVoucher")]
        public async Task<IActionResult> AddVoucher(string id, [FromBody] MVoucher request)
        {
            var result = await svc.AddVoucher(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetVoucherById/{voucherId}")]
        public async Task<IActionResult> GetVoucherById(string id, string voucherId)
        {
            var result = await svc.GetVoucherById(id, voucherId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteVoucherById/{voucherId}")]
        public async Task<IActionResult> DeleteVoucherById(string id, string voucherId)
        {
            var result = await svc.DeleteVoucherById(id, voucherId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableVoucherById/{voucherId}")]
        public async Task<IActionResult> EnableVoucherById(string id, string voucherId)
        {
            var result = await svc.UpdateVoucherStatusById(id, voucherId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableVoucherById/{voucherId}")]
        public async Task<IActionResult> DisableVoucherById(string id, string voucherId)
        {
            var result = await svc.UpdateVoucherStatusById(id, voucherId, "Disable");
            return Ok(result);
        }
    }
}
