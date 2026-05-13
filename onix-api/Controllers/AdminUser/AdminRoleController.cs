using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminRoleController : ControllerBase
    {
        private readonly IRoleService svc;

        [ExcludeFromCodeCoverage]
        public AdminRoleController(IRoleService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        // Use POST method, in the future we might send the body
        [HttpPost]
        [Route("org/global/action/GetRoles")]
        public IActionResult GetRoles([FromBody] VMRole param)
        {
            var result = svc.GetRoles("global", param);
            return Ok(result);
        }
    }
}
