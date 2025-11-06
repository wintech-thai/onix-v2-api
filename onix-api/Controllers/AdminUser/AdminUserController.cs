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
    [Route("/admin-api/[controller]")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService svc;

        [ExcludeFromCodeCoverage]
        public AdminUserController(IAdminUserService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/InviteUser")]
        public async Task<IActionResult> Inviteuser([FromBody] MAdminUser request)
        {
            var invitedByName = Response.HttpContext.Items["Temp-Identity-Name"];
            if (invitedByName == null)
            {
                invitedByName = "Unknown";
            }

            request.InvitedBy = invitedByName.ToString();

            var result = await svc.InviteUser(request);
            Response.Headers.Append("CUST_STATUS", result!.Status);
            Response.Headers.Append("CUST_DESC", result!.Description);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteUserById/{userId}")]
        public async Task<IActionResult> DeleteUserById(string userId)
        {
            var result = await svc.DeleteUserById(userId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetUserById/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await svc.GetUserById(userId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetUsers")]
        public async Task<IActionResult> GetUsers([FromBody] VMAdminUser param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetUsers(param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetUserCount")]
        public async Task<IActionResult> GetUserCount([FromBody] VMAdminUser param)
        {
            var result = await svc.GetUserCount(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateUserById/{userId}")]
        public async Task<IActionResult> UpdateUserById(string userId, [FromBody] MAdminUser request)
        {
            var result = await svc.UpdateUserById(userId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableUserById/{userId}")]
        public async Task<IActionResult> EnableUserById(string userId)
        {
            var result = await svc.UpdateUserStatusById(userId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableUserById/{userId}")]
        public async Task<IActionResult> DisableUserById(string userId)
        {
            var result = await svc.UpdateUserStatusById(userId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
    }
}
