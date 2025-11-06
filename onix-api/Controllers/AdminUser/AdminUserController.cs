using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;
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
/*
        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteUserById/{userId}")]
        public IActionResult DeleteUserById(string id, string userId)
        {
            var result = svc.DeleteUserById(id, userId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetUserById/{userId}")]
        public IActionResult GetUserById(string id, string userId)
        {
            var result = svc.GetUserByIdLeftJoin(id, userId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetUsers")]
        public IActionResult GetUsers(string id, [FromBody] VMOrganizationUser param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetUsersLeftJoin(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetUserCount")]
        public IActionResult GetUserCount(string id, [FromBody] VMOrganizationUser param)
        {
            var result = svc.GetUserCountLeftJoin(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateUserById/{userId}")]
        public IActionResult UpdateUserById(string id, string userId, [FromBody] MOrganizationUser request)
        {
            var result = svc.UpdateUserById(id, userId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableUserById/{userId}")]
        public IActionResult EnableUserById(string id, string userId)
        {
            var result = svc.UpdateUserStatusById(id, userId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableUserById/{userId}")]
        public IActionResult DisableUserById(string id, string userId)
        {
            var result = svc.UpdateUserStatusById(id, userId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
*/
    }
}
