using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminCustomRoleController : ControllerBase
    {
        private readonly ICustomRoleService svc;

        [ExcludeFromCodeCoverage]
        public AdminCustomRoleController(ICustomRoleService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetInitialUserRolePermissions")]
        public IActionResult GetInitialUserRolePermissions()
        {
            var result = svc.GetInitialAdminRolePermissions("global");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddCustomRole")]
        public async Task<IActionResult> AddCustomRole([FromBody] MCustomRole request)
        {
            var result = await svc.AddCustomRole("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteCustomRoleById/{customRoleId}")]
        public async Task<IActionResult> DeleteCustomRoleById(string customRoleId)
        {
            var result = await svc.DeleteCustomRoleById("global", customRoleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetCustomRoleById/{customRoleId}")]
        public async Task<IActionResult> GetCustomRoleById(string customRoleId)
        {
            var result = await svc.GetCustomRoleById("global", customRoleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateCustomRoleById/{customRoleId}")]
        public async Task<IActionResult> UpdateCustomRoleById(string customRoleId, [FromBody] MCustomRole request)
        {
            var result = await svc.UpdateCustomRoleById("global", customRoleId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetCustomRoles")]
        public async Task<IActionResult> GetCustomRoles([FromBody] VMCustomRole request)
        {
            var result = await svc.GetCustomRoles("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetCustomRoleCount")]
        public async Task<IActionResult> GetCustomRoleCount([FromBody] VMCustomRole request)
        {
            var result = await svc.GetCustomRoleCount("global", request);
            return Ok(result);
        }
    }
}
