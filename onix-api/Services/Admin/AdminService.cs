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
            var r = new MVOrganizeRegistration();
            //Send email noti to activate organization too
            return r;
        }
    }
}
