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
        
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdatePassword")]
        public IActionResult UpdatePassword(string id, [FromBody] MUpdatePassword request)
        {
            var idTypeObj = Response.HttpContext.Items["Temp-Identity-Type"];
            if (idTypeObj == null)
            {
                return BadRequest("Unable to identify identity type!!!");
            }

            var idType = idTypeObj.ToString();
            if (idType != "JWT")
            {
                return BadRequest("Only allow for JWT identity type!!!");
            }

            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj == null)
            {
                return BadRequest("Unable to find user name!!!");
            }

            var userName = nameObj.ToString();
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("User name is empty!!!");
            }

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยน password เฉพาะของตัวเองเท่านั้น
            var result = svc.UpdatePassword(userName, request);
            Response.Headers.Append("CUST_STATUS", result.Status);
            
            var message = $"{result.Description}";
            if (!string.IsNullOrEmpty(request.UserName) && (userName != request.UserName))
            {
                //เอาไว้ดูว่ามีใครลองส่ง username เข้ามาเพื่อ hack ระบบหรือไม่
                message = $"{message}, JWT user [{userName}] but injected user is [{request.UserName}]";
            }
            Response.Headers.Append("CUST_DESC", message);

            return Ok(result);
        }
    }
}
