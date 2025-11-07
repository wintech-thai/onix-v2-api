using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;
using System.Web;
using System.Text;

namespace Its.Onix.Api.Services
{
    public class AdminUserService : BaseService, IAdminUserService
    {
        private readonly IAdminUserRepository? repository = null;
        private readonly IUserRepository? userRepository = null;
        private readonly IJobService _jobService;
        private readonly IRedisHelper _redis;

        public AdminUserService(
            IAdminUserRepository repo,
            IUserRepository userRepo,
            IJobService jobService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            userRepository = userRepo;
            _jobService = jobService;
            _redis = redis;
        }

        public async Task<IEnumerable<MAdminUser>> GetUsers(VMAdminUser param)
        {
            var result = await repository!.GetUsersLeftJoin(param);
            return result;
        }

        public async Task<int> GetUserCount(VMAdminUser param)
        {
            var result = await repository!.GetUserCountLeftJoin(param);
            return result;
        }

        public async Task<MVAdminUser> GetUserById(string userId)
        {
            var r = new MVAdminUser()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(userId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"User ID [{userId}] format is invalid";

                return r;
            }

            var result = await repository!.GetUserByIdLeftJoin(userId);

            var au = result;
            if (au == null)
            {
                r.Status = "USER_ID_NOTFOUND";
                r.Description = $"User ID [{userId}] not found in our database!!!";

                return r;
            }

            if (!string.IsNullOrEmpty(au.RolesList))
            {
                au.Roles = [.. au.RolesList.Split(',')];
            }
            au.RolesList = "";
            r.AdminUser = au;

            return r;
        }

        public async Task<MVAdminUser?> DeleteUserById(string userId)
        {
            var r = new MVAdminUser()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(userId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"User ID [{userId}] format is invalid";

                return r;
            }

            var u = repository!.GetUserByIdLeftJoin(userId);
            if (u.Result == null)
            {
                r.Status = "NOTFOUND_GET_USER";
                r.Description = $"User ID [{userId}] not found!!!";

                return r;
            }

            var m = await repository!.DeleteUserById(userId);
            r.AdminUser = m;
            if (m == null)
            {
                r.Status = "NOTFOUND_DELTE_USER";
                r.Description = $"User ID [{userId}] not found!!!";
            }

            return r;
        }

        public async Task<MVAdminUser?> UpdateUserById(string userId, MAdminUser user)
        {
            var r = new MVAdminUser()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(userId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"User ID [{userId}] format is invalid";

                return r;
            }

            user.RolesList = string.Join(",", user.Roles ?? []);
            user.AdminUserId = Guid.Parse(userId);

            var result = await repository!.UpdateUserById(user);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{userId}] not found!!!";

                return r;
            }

            if (!string.IsNullOrEmpty(result.RolesList))
            {
                result.Roles = [.. result.RolesList.Split(',')];
            }

            r.AdminUser = result;

            return r;
        }

        public async Task<MVAdminUser?> UpdateUserStatusById(string userId, string status)
        {
            var r = new MVAdminUser()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(userId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Org user ID [{userId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateUserStatusById(userId, status);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{userId}] not found!!!";

                return r;
            }

            if (!string.IsNullOrEmpty(result.RolesList))
            {
                result.Roles = [.. result.RolesList.Split(',')];
            }

            r.AdminUser = result;
            return r;
        }

        public async Task<MVAdminUser?> UpdateUserStatusById(string adminUserId, string userId, string status)
        {
            var r = new MVAdminUser()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(adminUserId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Org user ID [{adminUserId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(userId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"User ID [{userId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateUserStatusById(adminUserId, userId, status);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{adminUserId}] not found!!!";

                return r;
            }

            if (!string.IsNullOrEmpty(result.RolesList))
            {
                result.Roles = [.. result.RolesList.Split(',')];
            }

            r.AdminUser = result;

            return r;
        }

        private string IdentifyRegistrationCase(MAdminUser user)
        {
            var userName = user.UserName!;
            var email = user.TmpUserEmail!;

            var userByNameObj = userRepository!.GetUserByName(userName);
            if (userByNameObj != null)
            {
                //มี username นั้นอยู่แล้วใน table Users
                var userEmail = userByNameObj.UserEmail;
                if (userEmail != email)
                {
                    //case3 : (username, email) มี username แต่ email ไม่ตรง => Error "username ถูกใช้โดย คนอื่นแล้ว"
                    return "ERROR_NAME_IS_USED_BY_ANOTHER";
                }

                //case1 : (username, email) มีอยู่แล้วใน table Users => Ok "สร้างใน OrganizationsUsers เท่านั้น"
                return "OK_TO_ADD_IN_ORG1";
            }

            //ยังไม่เคยมี username นี้อยู่ใน table Users เลย
            var userByEmailObj = userRepository!.GetUserByEmail(email);
            if (userByEmailObj == null)
            {
                //ยังไม่เคยมี username หรือ email นี้อยู่ใน table Users เลย
                //case2 : (username, email) ไม่มี username และ ไม่มี email เลย => Ok "สร้างใน table Users ด้วย"
                return "OK_TO_ADD_IN_ORG2";
            }

            //case4 : (username, email) ไมมี username แต่มี email ใน Users แล้ว => Error "Email ถูกใช้โดย user อื่นแล้ว"
            return "ERROR_EMAIL_IS_USED_BY_ANOTHER";
        }

        private MVJob? CreateEmailUserInvitationJob(string orgId, string regCase, MUserRegister reg)
        {
            var regType = "admin-signup-confirm";
            if (regCase == "OK_TO_ADD_IN_ORG1")
            {
                //เป็น link ที่ให้กด accept เท่านั้น ไม่ต้องให้กรอก user/password
                regType = "admin-invite-confirm";
            }

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

            var templateType = "admin-invitation";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "AdminUser.CreateEmailUserInvitationJob()",
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
                    new NameValue { Name = "REGISTRATION_URL", Value = registrationUrl },
                    new NameValue { Name = "INVITED_BY", Value = reg.InvitedBy },
                ]
            };

            var result = _jobService.AddJob(orgId, job);

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "AdminSignUp");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 12)); //หมดอายุ 12 ชั่วโมง

            return result;
        }

        public async Task<MVAdminUser?> InviteUser(MAdminUser user)
        {
            var r = new MVAdminUser()
            {
                Status = "OK",
                Description = "Success",
            };

            var userName = user.UserName!;
            var userValidateResult = ValidationUtils.ValidateUserName(userName);
            if (userValidateResult.Status != "OK")
            {
                r.Status = userValidateResult.Status;
                r.Description = userValidateResult.Description;

                return r;
            }

            //Validate if user exist in org
            var isUserExist = await repository!.IsUserNameExist(userName);
            if (isUserExist)
            {
                r.Status = "USERNAME_DUPLICATE";
                r.Description = $"User name [{userName}] is already exist!!!";

                return r;
            }

            var email = user.TmpUserEmail;
            if (string.IsNullOrEmpty(email))
            {
                r.Status = "INVALID_EMAIL_EMPTY";
                r.Description = "Email address is blank, please check your TmpUserEmail field!!!";
                return r;
            }

            //Validate email format
            var emailValidateResult = ValidationUtils.ValidateEmail(email);
            if (emailValidateResult.Status != "OK")
            {
                r.Status = emailValidateResult.Status;
                r.Description = emailValidateResult.Description;

                return r;
            }

            var registrationCase = IdentifyRegistrationCase(user);
            if (registrationCase.Contains("ERROR"))
            {
                r.Status = registrationCase;
                r.Description = "Email or username is being by used another!!!";

                return r;
            }

            user.UserStatus = "Pending";
            user.InvitedDate = DateTime.UtcNow;
            user.PreviousUserStatus = "Pending";
            user.RolesList = string.Join(",", user.Roles ?? []);

            var result = await repository!.AddUser(user);

            var tmpOrgId = "global";
            var reg = new MUserRegister()
            {
                Email = email,
                UserName = userName,
                OrgUserId = tmpOrgId,
                InvitedBy = user.InvitedBy,
            };
            CreateEmailUserInvitationJob(tmpOrgId, registrationCase, reg);

            r.AdminUser = result;
            //ป้องกันการ auto track กลับไปที่ column ใน table เลยต้อง assign result ให้กับ OrgUser ก่อน จากนั้นค่อยอัพเดต field อีกที
            r.AdminUser.RolesList = "";

            return r;
        }
        
        public MVAdminUser VerifyUserIsAdmin(string userName)
        {
            var u = userRepository!.GetUserByName(userName);
            if (u == null)
            {
                var o = new MVAdminUser()
                {
                    Status = "NOTFOUND",
                    Description = $"User [{userName}] not found !!!"
                };

                return o;
            }

            var t = repository!.GetUserByName(userName);
            var m = t.Result;

            if (m == null)
            {
                var o = new MVAdminUser()
                {
                    Status = "NOT_ADMIN_USER",
                    Description = $"User [{userName}] is not admin!!!",
                };

                return o;
            }

            if (m.UserStatus != "Active")
            {
                var o = new MVAdminUser()
                {
                    Status = "NOT_ACTIVE_STATUS_USER",
                    Description = $"User [{userName}] has status [{m.UserStatus}]!!!",
                };

                return o;
            }

            var mv = new MVAdminUser()
            {
                User = u,
                AdminUser = m,
                Status = "OK",
                Description = "Success",
            };

            return mv;
        }
    }
}
