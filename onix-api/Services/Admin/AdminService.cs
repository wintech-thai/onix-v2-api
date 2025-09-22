using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class AdminService : BaseService, IAdminService
    {
        private readonly IOrganizationService? orgService = null;
        private readonly IUserService userService;
        private readonly RedisHelper _redis;
        private readonly IJobService _jobService;

        public AdminService(IOrganizationService orgSvc,
            IUserService userSvc,
            IJobService jobService,
            RedisHelper redis) : base()
        {
            orgService = orgSvc;
            userService = userSvc;
            _jobService = jobService;
            _redis = redis;
        }

        private MVJob? CreateEmailSendOtpJob(string orgId, string emailOtp, string email)
        {
            var job = new MJob()
            {
                Name = $"EmailSendOtpJob:{Guid.NewGuid()}",
                Description = "Admin.CreateEmailSendOtpJob()",
                Type = "OtpEmailSend",
                Status = "Pending",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = "org-registration-otp" },
                    new NameValue { Name = "OTP", Value = emailOtp },
                ]
            };

            var result = _jobService.AddJob(orgId, job);
            return result;
        }

        public MVOtp SendOrgRegisterOtpEmail(string orgId, string email)
        {
            var r = new MVOtp()
            {
                Status = "SUCCESS",
                Description = "",
            };

            //Keep the email sent OTP in cache also for future use
            var emailOtp = ServiceUtils.CreateOTP(6);
            var o = new MOtp()
            {
                Otp = emailOtp,
            };
            var cacheKey1 = CacheHelper.CreateApiOtpKey(orgId, "SendOrgRegisterOtpEmail");
            _ = _redis.SetObjectAsync($"{cacheKey1}:{email}", o, TimeSpan.FromMinutes(30));

            //Submit job to send OTP email, use serial & pin (masking) in the email
            var jr = CreateEmailSendOtpJob(orgId, emailOtp, email);

            r.OTP = emailOtp;
            r.Description = $"OTP [{emailOtp}] sent to email [{email}]";

            return r;
        }

        public MVOrganizeRegistration RegisterOrganization(string orgId, MOrganizeRegistration user)
        {
            var email = user.Email;
            var userName = user.UserName;
            var userOrgName = user.UserOrgName;
            var userOtp = user.ProofEmailOtp;

            var r = new MVOrganizeRegistration()
            {
                Status = "SUCCESS",
                Description = $"Registered org=[{userOrgName}] for user=[{userName}]",
            };

            // ตรวจสอบว่า OTP ตรงกับที่เคยให้ออกไปก่อนหน้าหรือไม่
            var emailSentOtpCacheKey = CacheHelper.CreateApiOtpKey(orgId, "SendOrgRegisterOtpEmail");
            var emailSentOtpObj = _redis.GetObjectAsync<MOtp>($"{emailSentOtpCacheKey}:{email}").Result;
            if (emailSentOtpObj == null)
            {
                r.Status = "PROVIDED_OTP_NOTFOUND";
                r.Description = $"OTP [{userOtp}] for email=[{email}] not found or expire!!!";

                return r;
            }

            if (userOtp != emailSentOtpObj.Otp)
            {
                r.Status = "PROVIDED_OTP_INVALID";
                r.Description = $"OTP [{userOtp}] for email=[{email}] invalid (not match)!!!";

                return r;
            }

            // ตรวจสอบว่ามี username, useremail อยู่ในระบบก่อนหน้าหรือยัง

            // ตรวจสอบว่าชื่อ org_id ซ้ำหรือไม่

            // สร้าง org & user (สร้าง user ที่ Keycloak ด้วย)

            //Send email noti

            //Send email noti to activate organization too
            return r;
        }
    }
}
