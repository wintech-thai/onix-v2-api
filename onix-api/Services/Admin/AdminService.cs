using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class AdminService : BaseService, IAdminService
    {
        private readonly IOrganizationService _orgService;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IRedisHelper _redis;
        private readonly IJobService _jobService;

        public AdminService(IOrganizationService orgSvc,
            IUserService userSvc,
            IJobService jobService,
            IAuthService authService,
            IRedisHelper redis) : base()
        {
            _orgService = orgSvc;
            _userService = userSvc;
            _jobService = jobService;
            _authService = authService;
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

        private MVJob? CreateEmailSendWelcomeJob(string orgId, string email, string userOrgId, string userName)
        {
            var job = new MJob()
            {
                Name = $"EmailSendWelcomeJob:{Guid.NewGuid()}",
                Description = "Admin.CreateEmailSendWelcomeJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = "org-registration-welcome" },
                    new NameValue { Name = "USER_ORG_ID", Value = userOrgId },
                    new NameValue { Name = "ORG_USER_NAMME", Value = userName },
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
            var userOrgId = user.UserOrgId;
            var userOrgName = user.UserOrgName;
            var userOtp = user.ProofEmailOtp;

            var r = new MVOrganizeRegistration()
            {
                Status = "SUCCESS",
                Description = $"Registered org=[{userOrgId}] for user=[{userName}]",
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

            // ตรวจสอบว่ามี username
            var isUserExist = _userService.IsUserNameExist(orgId, userName!);
            if (isUserExist)
            {
                r.Status = "USER_ALREADY_EXIST";
                r.Description = $"User [{userName}] already exist!!!";

                return r;
            }

            // ตรวจสอบว่ามี useremail อยู่ในระบบก่อนหน้าหรือยัง
            var isEmailExist = _userService.IsEmailExist(orgId, email!);
            if (isEmailExist)
            {
                r.Status = "EMAIL_ALREADY_EXIST";
                r.Description = $"User [{email}] already exist!!!";

                return r;
            }

            // ตรวจสอบว่าชื่อ org_id ซ้ำหรือไม่
            var isOrgExist = _orgService.IsOrgIdExist(userOrgId!);
            if (isOrgExist)
            {
                r.Status = "ORG_ALREADY_EXIST";
                r.Description = $"Organization [{userOrgId}] already exist!!!";

                return r;
            }

            // สร้าง org
            var org = new MOrganization()
            {
                OrgCustomId = userOrgId,
                OrgName = userOrgName,
                OrgDescription = user.UserOrgDesc,
            };
            var orgResult = _orgService.AddOrganization("notused", org);
            if (orgResult.Status != "OK")
            {
                r.Status = orgResult.Status;
                r.Description = orgResult.Description;

                return r;
            }

            // สร้าง user
            var usr = new MUser()
            {
                UserName = userName,
                UserEmail = email,
                IsOrgInitialUser = "YES",
            };
            var usrResult = _userService.AddUser("notused", usr);
            if (usrResult.Status != "OK")
            {
                r.Status = usrResult.Status;
                r.Description = usrResult.Description;

                return r;
            }

            //เพิ่ม user เข้า Organization
            var orgUser = new MOrganizationUser()
            {
                UserName = userName,
                UserEmail = email,
                UserId = usrResult.User!.UserId.ToString(), //ได้ค่า ID มาตอนที่ add user ก่อนหน้า
                RolesList = "OWNER",
            };
            var usrAddOrgResult = _orgService.AddUserToOrganization(userOrgId!, orgUser);
            if (usrAddOrgResult.Status != "OK")
            {
                r.Status = orgResult.Status;
                r.Description = orgResult.Description;

                return r;
            }

            var idpResult = _authService.AddUserToIDP(user).Result;
            if (!idpResult.Success)
            {
                r.Status = "IDP_ADD_USER_ERROR";
                r.Description = idpResult.Message;

                return r;
            }

            //Send email noti to activate organization too
            CreateEmailSendWelcomeJob(orgId, email!, userOrgId!, userName!);

            return r;
        }

        public bool IsOrganizationExist(string orgId)
        {
            var isOrgExist = _orgService.IsOrgIdExist(orgId!);
            return isOrgExist;
        }

        public bool IsUserNameExist(string userName)
        {
            var isUserExist = _userService.IsUserNameExist("notused", userName);
            return isUserExist;
        }

        public bool IsEmailExist(string email)
        {
            var isEmailExist = _userService.IsEmailExist("notused", email);
            return isEmailExist;
        }
    }
}
