using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Controllers
{
    public class CustomerIdentityValidationResult
    {
        public string? UserName { get; set; }
        public string? CustomerId { get; set; }
        public ObjectResult? RequestResult { get; set; }
    }

    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/customer-api/[controller]")]
    public class OnlyCustomerUserController : ControllerBase
    {
        private readonly IUserService svc;
        private readonly IOrganizationService _orgSvc;
        private readonly IRedisHelper _redis;

        public OnlyCustomerUserController(
            IUserService service,
            IRedisHelper redis,
            IOrganizationService orgService)
        {
            svc = service;
            _orgSvc = orgService;
            _redis = redis;
        }

        private IdentityValidationResult ValidateUserIdentity()
        {
            var result = new IdentityValidationResult();

            var idTypeObj = Response.HttpContext.Items["Temp-Identity-Type"];
            if (idTypeObj == null)
            {
                var obj = BadRequest("Unable to identify identity type!!!");
                result.RequestResult = obj;

                return result;
            }

            var idType = idTypeObj.ToString();
            if (idType != "JWT")
            {
                var obj = BadRequest("Only allow for JWT identity type!!!");
                result.RequestResult = obj;

                return result;
            }

            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj == null)
            {
                var obj = BadRequest("Unable to find user name!!!");
                result.RequestResult = obj;

                return result;
            }

            var userName = nameObj.ToString();
            if (userName == "")
            {
                var obj = BadRequest("User name is empty!!!");
                result.RequestResult = obj;

                return result;
            }

            result.UserName = userName;

            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/UpdatePassword")]
        public IActionResult UpdatePassword(string id, [FromBody] MUpdatePassword request)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var userName = validateResult.UserName;
            request.UserName = userName;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยน password เฉพาะของตัวเองเท่านั้น
            var result = svc.UpdatePassword(userName, request);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var message = $"{result.Description}";
            if (!string.IsNullOrEmpty(request.UserName) && (userName != request.UserName))
            {
                //เอาไว้ดูว่ามีใครลองส่ง username เข้ามาเพื่อ hack ระบบหรือไม่
                message = $"{message}, JWT user [{userName}] but injected user is [{request.UserName}]";
            }
            //Comment ไว้ก่อนเพราะถ้า validation password ผิด มันจะมีอักขระพิเศษที่ใส่ใน header ไม่ได้ 
            //Response.Headers.Append("CUST_DESC", message);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateUserInfo")]
        public IActionResult UpdateUserInfo(string id, [FromBody] MUser request)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var uname = validateResult.UserName;
            request.UserName = uname;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยนข้อมูลเฉพาะของตัวเองเท่านั้น
            var result = svc.UpdateUserByUserName(uname, request);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var message = $"{result.Description}";
            if (!string.IsNullOrEmpty(request.UserName) && (uname != request.UserName))
            {
                //เอาไว้ดูว่ามีใครลองส่ง username เข้ามาเพื่อ hack ระบบหรือไม่
                message = $"{message}, JWT user [{uname}] but injected user is [{request.UserName}]";
            }

            Response.Headers.Append("CUST_DESC", message);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetUserInfo")]
        public IActionResult GetUserInfo(string id)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var uname = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยนข้อมูลเฉพาะของตัวเองเท่านั้น
            var result = svc.GetUserByUserName(uname);
            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/Logout")]
        public IActionResult Logout(string id)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var userName = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้น
            var result = svc.UserLogout(userName);
            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);

            var sessionKey = CacheHelper.CreateLoginSessionKey(userName);
            _ = _redis.DeleteAsync(sessionKey);

            return Ok(result);
        }
    }
}
