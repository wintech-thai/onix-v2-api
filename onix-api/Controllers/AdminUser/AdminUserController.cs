using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;
using System.Text;
using System.Web;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService svc;
        private readonly IRedisHelper _redis;

        [ExcludeFromCodeCoverage]
        public AdminUserController(IAdminUserService service, IRedisHelper redis)
        {
            svc = service;
            _redis = redis;
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
        [HttpPost]
        [Route("org/global/action/InviteUserWithLink")]
        public async Task<IActionResult> InviteUserWithLink([FromBody] MAdminUser request)
        {
            var invitedByName = Response.HttpContext.Items["Temp-Identity-Name"];
            if (invitedByName == null)
            {
                invitedByName = "Unknown";
            }

            request.InvitedBy = invitedByName.ToString();

            var result = await svc.InviteUserWithLink(request);
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

        private string CreateForgotPasswordLink(string orgId, MUserRegister reg)
        {
            var regType = "forgot-password";

            var jsonString = JsonSerializer.Serialize(reg);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);

            var dataUrlSafe = HttpUtility.UrlEncode(jsonStringB64);

            var registerDomain = "<REGISTER_SERVICE_DOMAIN>"; //คนที่เรียกใช้งานจะต้องเปลี่ยนเป็น domain ของ register service เอง

            var token = Guid.NewGuid().ToString();
            var registrationUrl = $"https://{registerDomain}/{regType}/{orgId}/{token}?data={dataUrlSafe}";

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "AdminForgotPassword");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return registrationUrl;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetForgotPasswordLink/{userId}")]
        public async Task<IActionResult> GetForgotPasswordLink(string userId)
        {
            var id = "global";

            //ต้องใช้งานอย่างระมัดระวัง อย่างไป grant สิทธ์ให้ user แบบมั่ว ๆ ซั่ว ๆ นะ
            //จริง ๆ ควรต้องส่งไปยัง email เลยแต่ ณ ตอนนี้ไม่มีระบบ email
            var mv = new MVOrganizationUserRegistration()
            {
                Status = "OK",
                Description = "Success"
            };

            var mvAdminUser = await svc.GetUserById(userId);
            if (mvAdminUser.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", mvAdminUser.Status);
                return Ok(mvAdminUser);
            }

            var user = mvAdminUser.AdminUser!;
            if (user == null)
            {
                mv.Status = "EMPTY_USER_RETURN";
                mv.Description = $"No user return for org user ID [{userId}] !!!";

                Response.Headers.Append("CUST_STATUS", mv.Status);
                return Ok(mv);
            }

            if (user.UserStatus != "Active")
            {
                mv.Status = "USER_NOT_ACTIVE";
                mv.Description = $"User status is [{user.UserStatus}] for org user ID [{userId}] !!!";

                Response.Headers.Append("CUST_STATUS", mv.Status);
                return Ok(mv);
            }

            var reg = new MUserRegister()
            {
                Email = user.UserEmail,
                UserName = user.UserName!,
                OrgUserId = id,
            };

            var forgotPasswordUrl = CreateForgotPasswordLink(id, reg);
            mv.ForgotPasswordUrl = forgotPasswordUrl;

            Response.Headers.Append("CUST_STATUS", mv.Status);
            return Ok(mv);
        }
    }
}
