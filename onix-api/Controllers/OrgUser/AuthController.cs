using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService svc;
        private readonly IJobService _jobSvc;
        private readonly IUserService _userSvc;
        private readonly IEntityService _entitySvc;
        private readonly IRedisHelper _redis;

        [ExcludeFromCodeCoverage]
        public AuthController(
            IAuthService service,
            IRedisHelper redis,
            IJobService jobSvc,
            IUserService userSvc,
            IEntityService entitySvc
            )
        {
            svc = service;
            _redis = redis;
            _jobSvc = jobSvc;
            _entitySvc = entitySvc;
            _userSvc = userSvc;
        }

        private MVJob? CreateEmailForgotPasswordJob(string orgId, MUserRegister reg)
        {
            var regType = "forgot-password";

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

            var templateType = "user-forgot-password";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "Auth.CreateEmailForgotPasswordJob()",
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
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "UserForgotPassword");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return result;
        }
        
        private MVJob? CreateCustomerEmailForgotPasswordJob(string orgId, MUserRegister reg)
        {
            var regType = "customer-forgot-password";

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

            var templateType = "customer-forgot-password";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "Auth.CreateCustomerEmailForgotPasswordJob()",
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
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "CustomerForgotPassword");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/SendForgotPasswordEmail/{email}")]
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
        

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SendCustomerForgotPasswordEmail/{email}")]
        public IActionResult SendCustomerForgotPasswordEmail(string id, string email)
        {
            var mv = new MVRegistration()
            {
                Status = "OK",
                Description = "Success"
            };

            var customer = _entitySvc.GetEntityByEmail(id, email);
            if (customer == null)
            {
                mv.Status = "EMAIL_NOT_FOUND";
                mv.Description = "Email not found in database";

                Response.Headers.Append("CUST_STATUS", mv.Status);
                return Ok(mv);
            }

            var reg = new MUserRegister()
            {
                Email = email,
                UserName = customer.PrimaryEmail!, // assuming PrimaryEmail is the username
                OrgUserId = customer.Id!.ToString(),
            };
            var result = CreateCustomerEmailForgotPasswordJob(id, reg);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/Login")]
        public IActionResult Login([FromBody] UserLogin request)
        {
            var result = svc.Login(request);
            Response.HttpContext.Items.Add("Temp-Identity-Name", request.UserName);
            
            if (result.Status != "Success")
            {
                return Unauthorized("Unauthorized, incorrect user or password!!!");
            }

            var sessionKey = CacheHelper.CreateLoginSessionKey(request.UserName);
            var obj = new UserToken() { UserName = request.UserName };
            _ = _redis.SetObjectAsync(sessionKey, obj);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/Refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = svc.RefreshToken(request.RefreshToken);
            Response.HttpContext.Items.Add("Temp-Identity-Name", result.UserName);

            var sessionKey = CacheHelper.CreateLoginSessionKey(result.UserName);

            if (result.Status != "Success")
            {
                _ = _redis.DeleteAsync(sessionKey);
                return Unauthorized("Unauthorized, incorrect refresh token!!!");
            }

            var obj = new UserToken() { UserName = result.UserName };
            _ = _redis.SetObjectAsync(sessionKey, obj);

            return Ok(result);
        }
    }
}
