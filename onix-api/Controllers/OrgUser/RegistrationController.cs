using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRedisHelper _redis;
        private readonly IAuthService _authService;
        private readonly IOrganizationUserService _orgUserService;
        private readonly IJobService _jobService;
        private readonly IEntityService _entityService;

        public RegistrationController(IUserService userService,
            IAuthService authService,
            IOrganizationUserService orgUserService,
            IJobService jobService,
            IEntityService entityService,
            IRedisHelper redis)
        {
            _userService = userService;
            _redis = redis;
            _authService = authService;
            _orgUserService = orgUserService;
            _jobService = jobService;
            _entityService = entityService;
        }

        private MVJob? CreateEmailUserGreetingJob(string orgId, MUserRegister reg)
        {
            var consoleDomain = "console";
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                consoleDomain = "console-dev";
            }

            var consoleUrl = $"https://{consoleDomain}.please-scan.com";

            var templateType = "user-invitation-to-org-welcome";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "Registration.CreateEmailUserGreetingJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",
                Tags = templateType,

                Parameters =
                [
                    new NameValue { Name = "EMAIL_NOTI_ADDRESS", Value = "pjame.fb@gmail.com" },
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = reg.Email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new NameValue { Name = "ORG_USER_NAMME", Value = reg.UserName },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                    new NameValue { Name = "CONSOLE_URL", Value = consoleUrl },
                ]
            };

            var result = _jobService.AddJob(orgId, job);
            return result;
        }

        private MVRegistration ValidateRegistrationToken(string usrName, MUserRegister request, string cacheKey)
        {
            var result = new MVRegistration()
            {
                Status = "OK",
                Description = "Valid token",
            };

            var cacheObj = _redis.GetObjectAsync<MUserRegister>(cacheKey);
            var ur = cacheObj.Result;

            if (ur == null)
            {
                result.Status = "INVALID_TOKEN_OR_EXPIRED";
                result.Description = "Invalid or expired token";

                return result;
            }

            if (ur.UserName != usrName)
            {
                result.Status = "USERNAME_MISMATCH_TOKEN";
                result.Description = "Username does not match the token";

                return result;
            }

            if (ur.Email != request.Email)
            {
                result.Status = "EMAIL_MISMATCH_TOKEN";
                result.Description = "Email does not match the token";

                return result;
            }

            if (ur.OrgUserId != request.OrgUserId)
            {
                result.Status = "USERID_MISMATCH_TOKEN";
                result.Description = "User ID does not match the token";

                return result;
            }

            return result;
        }
        
        private MVRegistration ValidateCustomerEmailVerificationToken(string custId, string cacheKey)
        {
            var result = new MVRegistration()
            {
                Status = "OK",
                Description = "Valid token",
            };

            var cacheObj = _redis.GetObjectAsync<MEmailVerification>(cacheKey);
            var ur = cacheObj.Result;

            if (ur == null)
            {
                result.Status = "INVALID_TOKEN_OR_EXPIRED";
                result.Description = "Invalid or expired token";

                return result;
            }

            if (ur.Id != custId)
            {
                result.Status = "CUST_ID_MISMATCH_TOKEN";
                result.Description = "Customer ID does not match the token";

                return result;
            }

            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/ConfirmExistingUserInvitation/{token}/{userName}")]
        public IActionResult ConfirmExistingUserInvitation(string id, string token, string userName, [FromBody] MUserRegister request)
        {
            var cacheSuffix = CacheHelper.CreateApiOtpKey(id, "UserSignUp");
            var cacheKey = $"{cacheSuffix}:{token}";

            var v = ValidateRegistrationToken(userName, request, cacheKey);
            if (v.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var userValidateResult = ValidationUtils.ValidateUserName(userName);
            if (userValidateResult.Status != "OK")
            {
                v.Status = userValidateResult.Status;
                v.Description = userValidateResult.Description;

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            //Get user by user name เพื่อเอาค่า userId มาอัพเดตใน OrgUser
            var mUser = _userService.GetUserByName(id, userName);
            if (mUser == null)
            {
                v.Status = "USER_NOTFOUND";
                v.Description = $"User name [{userName}] not found";

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var newUserId = mUser.UserId!.ToString();
            _ = _orgUserService.UpdateUserStatusById(id, request.OrgUserId!, newUserId!, "Active");

            //สร้าง email แจ้ง user ว่าการสมัครเสร็จสมบูรณ์
            CreateEmailUserGreetingJob(id, request);

            //ลบ cache ทิ้ง เพราะใช้แล้ว, และเพื่อกันไม่ให้กด link เดิมได้อีก
            _redis.DeleteAsync(cacheKey);

            v.User = request;

            Response.Headers.Append("CUST_STATUS", v.Status);
            return Ok(v);
        }

        [HttpPost]
        [Route("org/{id}/action/ConfirmNewUserInvitation/{token}/{userName}")]
        public IActionResult ConfirmNewUserInvitation(string id, string token, string userName, [FromBody] MUserRegister request)
        {
            var cacheSuffix = CacheHelper.CreateApiOtpKey(id, "UserSignUp");
            var cacheKey = $"{cacheSuffix}:{token}";

            var v = ValidateRegistrationToken(userName, request, cacheKey);
            if (v.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var userValidateResult = ValidationUtils.ValidateUserName(userName);
            if (userValidateResult.Status != "OK")
            {
                v.Status = userValidateResult.Status;
                v.Description = userValidateResult.Description;

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var validateResult = ValidationUtils.ValidatePassword(request.Password!);
            if (validateResult.Status != "OK")
            {
                v.Status = validateResult.Status;
                v.Description = validateResult.Description;

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var mvUser = _userService.AddUser(id, new MUser()
            {
                UserEmail = request.Email,
                UserName = request.UserName,
            });

            if (mvUser.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", mvUser.Status);
                return Ok(mvUser);
            }

            var newUserId = mvUser!.User!.UserId!.ToString();

            var mvOu = _orgUserService.UpdateUserStatusById(id, request.OrgUserId!, newUserId!, "Active");
            if (mvOu!.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", mvOu.Status);
                return Ok(mvOu);
            }

            //Call AuthService to add user/password to IDP
            var orgUser = new MOrganizeRegistration()
            {
                UserName = request.UserName!,
                UserInitialPassword = request.Password!,
                Name = request.Name,
                Lastname = request.Lastname,
                Email = request.Email,
            };
            var addUserTask = _authService.AddUserToIDP(orgUser);

            var idpResult = addUserTask.Result;
            if (!idpResult.Success)
            {
                v.Status = "IDP_USER_ADD_FAILED";
                v.Description = $"Failed to add user to IDP. Message: {idpResult.Message}";

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            //สร้าง email แจ้ง user ว่าการสมัครเสร็จสมบูรณ์
            CreateEmailUserGreetingJob(id, request);

            //ลบ cache ทิ้ง เพราะใช้แล้ว, และเพื่อกันไม่ให้กด link เดิมได้อีก
            _redis.DeleteAsync(cacheKey);

            Response.Headers.Append("CUST_STATUS", mvOu.Status);
            return Ok(mvOu);
        }

        [HttpPost]
        [Route("org/{id}/action/ConfirmCustomerEmailVerification/{token}/{custId}")]
        public IActionResult ConfirmNewUserInvitation(string id, string token, string custId)
        {
            var cacheSuffix = CacheHelper.CreateApiOtpKey(id, "CustomerEmailVerification");
            var cacheKey = $"{cacheSuffix}:{token}";

            var v = ValidateCustomerEmailVerificationToken(custId, cacheKey);
            if (v.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var mvCust = _entityService.UpdateEntityEmailStatusById(id, custId, "VERIFIED");
            if (mvCust!.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", mvCust.Status);
                return Ok(mvCust);
            }

            //ลบ cache ทิ้ง เพราะใช้แล้ว, และเพื่อกันไม่ให้กด link เดิมได้อีก
            _redis.DeleteAsync(cacheKey);

            Response.Headers.Append("CUST_STATUS", mvCust.Status);
            return Ok(mvCust);
        }

        [HttpPost]
        [Route("org/{id}/action/ConfirmForgotPasswordReset/{token}/{userName}")]
        public IActionResult ConfirmForgotPasswordReset(string id, string token, string userName, [FromBody] MUserRegister request)
        {
            var cacheSuffix = CacheHelper.CreateApiOtpKey(id, "UserForgotPassword");
            var cacheKey = $"{cacheSuffix}:{token}";

            var v = ValidateRegistrationToken(userName, request, cacheKey);
            if (v.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            var validateResult = ValidationUtils.ValidatePassword(request.Password!);
            if (validateResult.Status != "OK")
            {
                v.Status = validateResult.Status;
                v.Description = validateResult.Description;

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            //Call AuthService to update password to IDP
            var user = new MUpdatePassword()
            {
                UserName = request.UserName!,
                NewPassword = request.Password!,
            };
            var passwordChangeTask = _authService.ChangeForgotUserPasswordIdp(user);

            var idpResult = passwordChangeTask.Result;
            if (!idpResult.Success)
            {
                v.Status = "IDP_UPDATE_PASSWORD_FAILED";
                v.Description = $"Failed to update password to IDP. Message: {idpResult.Message}";

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            //สร้าง email แจ้ง user ว่า Password เปลี่ยนเรียบร้อยแล้ว
            _userService.CreateEmailPasswordChangeJob(id, request.Email!, userName);

            //ลบ cache ทิ้ง เพราะใช้แล้ว, และเพื่อกันไม่ให้กด link เดิมได้อีก
            _redis.DeleteAsync(cacheKey);

            Response.Headers.Append("CUST_STATUS", v.Status);
            return Ok(v);
        }


        [HttpPost]
        [Route("org/{id}/action/ConfirmCreateCustomerUser/{token}/{custId}")]
        public IActionResult ConfirmCreateCustomerUser(string id, string token, string custId,[FromBody] MUserRegister request)
        {
            //ความตั้งใจคือต้องมี customer entity อยู่แล้ว ก่อนถึงจะยืนยัน email ได้
            //request ส่งเข้ามาแค่ Password เพื่อสร้าง user เท่านั้น
            var cacheSuffix = CacheHelper.CreateApiOtpKey(id, "ConfirmCreateCustomerUser");
            var cacheKey = $"{cacheSuffix}:{token}";

            var v = ValidateCustomerEmailVerificationToken(custId, cacheKey);
            if (v.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            //TODO : ในอนาคตสามารถสร้าง entity จากตรงนี้หากยังไม่มีเพื่อให้ได้ custId แทนจากที่ส่งมา

            //สร้าง user ไปที่ Users และ IDP โดย username = customer:<org_id>:<custId>
            var userName = $"customer:{id}:{custId}";
            var mvCustUser = _entityService.UpdateEntityUserNameById(id, custId, userName);
            if (mvCustUser!.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", mvCustUser.Status);
                return Ok(mvCustUser);
            }

            //Update user_name ใน entity record ด้วย, รวมถึง user_status = Active 
            var mvCustStatus = _entityService.UpdateEntityUserStatusById(id, custId, userName);
            if (mvCustStatus!.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", mvCustStatus.Status);
                return Ok(mvCustStatus);
            }

            //Call AuthService to add user/password to IDP
            var customer = _entityService.GetEntityById(id, custId);
            var orgUser = new MOrganizeRegistration()
            {
                UserName = userName,
                UserInitialPassword = request.Password!,
                Name = customer.Name,
                Lastname = customer.Name,
                Email = customer.PrimaryEmail,
            };
            var addUserTask = _authService.AddUserToIDP(orgUser);

            var idpResult = addUserTask.Result;
            if (!idpResult.Success)
            {
                v.Status = "IDP_USER_ADD_FAILED";
                v.Description = $"Failed to add user to IDP. Message: {idpResult.Message}";

                Response.Headers.Append("CUST_STATUS", v.Status);
                return Ok(v);
            }

            //ลบ cache ทิ้ง เพราะใช้แล้ว, และเพื่อกันไม่ให้กด link เดิมได้อีก
            _redis.DeleteAsync(cacheKey);

            Response.Headers.Append("CUST_STATUS", mvCustStatus.Status);
            return Ok(mvCustStatus);
        }
    }
}
