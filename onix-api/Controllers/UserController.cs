using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService svc;

        [ExcludeFromCodeCoverage]
        public UserController(IUserService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/AdminGetUsers")]
        public IActionResult AdminGetUsers(string id)
        {
            var result = svc.GetUsers(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AdminAddUser")]
        public IActionResult AdminAddUser(string id, [FromBody] MUser request)
        {
            var result = svc.AddUser(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddUser")]
        public IActionResult AddUser(string id, [FromBody] MUser request)
        {
            var result = svc.AddUser(id, request);
            if (result!.Status != "OK")
            {
                return BadRequest(result!.Description);
            }
            
            return Ok(result);
        }
    }
}
