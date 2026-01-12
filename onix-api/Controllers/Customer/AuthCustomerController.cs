using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Route("/customer-api/[controller]")]
    public class AuthCustomerController : ControllerBase
    {
        private readonly IAuthService svc;
        private readonly IEntityService _entitySvc;
        private readonly IJobService _jobSvc;
        private readonly IUserService _userSvc;
        private readonly IRedisHelper _redis;

        [ExcludeFromCodeCoverage]
        public AuthCustomerController(
            IAuthService service,
            IEntityService entitySvc,
            IRedisHelper redis,
            IJobService jobSvc,
            IUserService userSvc
            )
        {
            svc = service;
            _redis = redis;
            _jobSvc = jobSvc;
            _userSvc = userSvc;
            _entitySvc = entitySvc;
        }
/*
        private MVJob? CreateEmailForgotPasswordJob(string orgId, MUserRegister reg)
        {
            var regType = "admin-forgot-password";

            var jsonString = JsonSerializer.Serialize(reg);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);

            var dataUrlSafe = HttpUtility.UrlEncode(jsonStringB64);

            var registerDomain = "register";
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                registerDomain = "register-dev";
            }

            var token = Guid.NewGuid().ToString();
            var registrationUrl = $"https://{registerDomain}.please-scan.com/{orgId}/{regType}/{token}?data={dataUrlSafe}";

            var templateType = "admin-forgot-password";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "AuthAdmin.CreateEmailForgotPasswordJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",
                Tags = templateType,

                Parameters =
                [
                    new NameValue { Name = "EMAIL_NOTI_ADDRESS", Value = "pjame.fb@gmail.com" },
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = reg.Email },
                    new NameValue { Name = "USER_NAME", Value = reg.UserName },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                    new NameValue { Name = "RESET_PASSWORD_URL", Value = registrationUrl },
                ]
            };

            var result = _jobSvc.AddJob(orgId, job);

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "AdminForgotPassword");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return result;
        }
        
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SendForgotPasswordEmail/{email}")]
        public IActionResult SendForgotPasswordEmail(string email)
        {
            var mv = new MVRegistration()
            {
                Status = "OK",
                Description = "Success"
            };

            var user = _userSvc.GetUserByEmail("notused", email);
            if (user == null)
            {
                mv.Status = "EMAIL_NOT_FOUND";
                mv.Description = "Email not found in database";

                Response.Headers.Append("CUST_STATUS", mv.Status);
                return Ok(mv);
            }

            var reg = new MUserRegister()
            {
                Email = email,
                UserName = user.UserName!,
                OrgUserId = user.UserId!.ToString(),
            };
            var result = CreateEmailForgotPasswordJob("temp", reg);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }
*/
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/Login")]
        public async Task<IActionResult> Login(string id, [FromBody] UserLogin request)
        {
            //ใช้ request.UserName ที่เป็น email เข้ามาเป็น login
            var cust = _entitySvc.GetEntityByEmail(id, request.UserName);
            if (cust == null)
            {
                return Unauthorized("Unauthorized, incorrect user or password!!!");
            }

            var internalUserName = $"customer:{id}:{cust.Id}";
            request.UserName = internalUserName;

            var result = svc.Login(request);
            Response.HttpContext.Items.Add("Temp-Identity-Name", request.UserName);
            
            if (result.Status != "Success")
            {
                return Unauthorized("Unauthorized, incorrect user or password!!!");
            }

            var sessionKey = CacheHelper.CreateCustomerLoginSessionKey(request.UserName);
            var obj = new UserToken() { UserName = request.UserName };
            await _redis.SetObjectAsync(sessionKey, obj);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/Refresh")]
        public async Task<IActionResult> Refresh(string id, [FromBody] RefreshTokenRequest request)
        {
            var result = svc.RefreshToken(request.RefreshToken);
            Response.HttpContext.Items.Add("Temp-Identity-Name", result.UserName);

            var sessionKey = CacheHelper.CreateCustomerLoginSessionKey(result.UserName);

            if (result.Status != "Success")
            {
                _ = await _redis.DeleteAsync(sessionKey);
                return Unauthorized("Unauthorized, incorrect refresh token!!!");
            }

            var obj = new UserToken() { UserName = result.UserName };
            await _redis.SetObjectAsync(sessionKey, obj);

            return Ok(result);
        }
    }
}
