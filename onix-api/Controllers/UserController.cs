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

        public UserController(IUserService service)
        {
            svc = service;
        }

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
