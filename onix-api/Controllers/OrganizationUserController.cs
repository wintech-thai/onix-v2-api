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
    [Route("/api/[controller]")]
    public class OrganizationUserController : ControllerBase
    {
        private readonly IOrganizationUserService svc;

        [ExcludeFromCodeCoverage]
        public OrganizationUserController(IOrganizationUserService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddUser")]
        public MVOrganizationUser? AddUser(string id, [FromBody] MOrganizationUser request)
        {
            var result = svc.AddUser(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteUserById/{userId}")]
        public IActionResult DeleteUserById(string id, string userId)
        {
            var result = svc.DeleteUserById(id, userId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetUserById/{userId}")]
        public MOrganizationUser GetUserById(string id, string userId)
        {
            var result = svc.GetUserById(id, userId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetUsers")]
        public IActionResult GetUsers(string id, [FromBody] VMOrganizationUser param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetUsers(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetUserCount")]
        public IActionResult GetUserCount(string id, [FromBody] VMOrganizationUser param)
        {
            var result = svc.GetUserCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateUserById/{userId}")]
        public IActionResult UpdateUserById(string id, string userId, [FromBody] MOrganizationUser request)
        {
            var result = svc.UpdateUserById(id, userId, request);
            return Ok(result);
        }
    }
}
