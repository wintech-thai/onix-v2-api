using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class OnlyAdminController : ControllerBase
    {
        private readonly IUserService svc;
        private readonly IRedisHelper _redis;

        public OnlyAdminController(
            IUserService service,
            IRedisHelper redis,
            IOrganizationService orgService)
        {
            svc = service;
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
        [Route("org/global/action/UpdatePassword")]
        public IActionResult UpdatePassword([FromBody] MUpdatePassword request)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var userName = validateResult.UserName;
            request.UserName = userName;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยน password เฉพาะของตัวเองเท่านั้น
            var result = svc.UpdatePassword("global", userName, request);
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
        [Route("org/global/action/UpdateUserInfo")]
        public IActionResult UpdateUserInfo([FromBody] MUser request)
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
        [Route("org/global/action/GetUserInfo")]
        public IActionResult GetUserInfo()
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
        [Route("org/global/action/Logout")]
        public IActionResult Logout()
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
