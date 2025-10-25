using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using onix.api.Migrations;
using System.Text.Json;
using System.Web;
using System.Text;

namespace Its.Onix.Api.Services
{
    public class OrganizationUserService : BaseService, IOrganizationUserService
    {
        private readonly IOrganizationUserRepository? repository = null;
        private readonly IUserRepository? userRepository = null;
        private readonly IJobService _jobService;
        private readonly IRedisHelper _redis;

        public OrganizationUserService(
            IOrganizationUserRepository repo,
            IUserRepository userRepo,
            IJobService jobService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            userRepository = userRepo;
            _jobService = jobService;
            _redis = redis;
        }

        public IEnumerable<MOrganizationUser> GetUsers(string orgId, VMOrganizationUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUsers(param);

            return result;
        }

        public IEnumerable<MOrganizationUser> GetUsersLeftJoin(string orgId, VMOrganizationUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUsersLeftJoin(param);

            return result;
        }

        public MVOrganizationUser? AddUser(string orgId, MOrganizationUser user)
        {
            repository!.SetCustomOrgId(orgId);
            userRepository!.SetCustomOrgId(orgId);

            var u = new MUser()
            {
                UserName = user.UserName,
                UserEmail = user.UserEmail,
            };

            var userAdded = userRepository!.AddUser(u);
            user.UserId = userAdded.UserId.ToString();

            var result = repository!.AddUser(user);

            var r = new MVOrganizationUser();
            r.Status = "OK";
            r.Description = "Success";
            r.OrgUser = result;

            return r;
        }

        private MVJob? CreateEmailUserInvitationJob(string orgId, string regCase, MUserRegister reg)
        {
            var regType = "user-signup-confirm";
            if (regCase == "OK_TO_ADD_IN_ORG1")
            {
                //เป็น link ที่ให้กด accept เท่านั้น ไม่ต้องให้กรอก user/password
                regType = "user-invite-confirm";
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

            var job = new MJob()
            {
                Name = $"EmailUserInvitationJob:{Guid.NewGuid()}",
                Description = "OrgUser.CreateEmailUserInvitationJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_NOTI_ADDRESS", Value = "pjame.fb@gmail.com" },
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = reg.Email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = "user-invitation-to-org" },
                    new NameValue { Name = "ORG_USER_NAMME", Value = reg.UserName },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                    new NameValue { Name = "REGISTRATION_URL", Value = registrationUrl },
                    new NameValue { Name = "INVITED_BY", Value = reg.InvitedBy },
                ]
            };

            var result = _jobService.AddJob(orgId, job);

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "UserSignUp");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return result;
        }

        public string IdentifyRegistrationCase(MOrganizationUser user)
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

        public MVOrganizationUser? InviteUser(string orgId, MOrganizationUser user)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVOrganizationUser()
            {
                Status = "OK",
                Description = "Success",
            };

            var userName = user.UserName;
            if (string.IsNullOrEmpty(userName))
            {
                r.Status = "INVALID_USERNAME_EMPTY";
                r.Description = "Username is blank, please check your UserName field!!!";
                return r;
            }

            //Validate if user exist in org
            var isUserExist = repository!.IsUserNameExist(userName);
            if (isUserExist)
            {
                r.Status = "USERNAME_DUPLICATE";
                r.Description = $"User name [{userName}] is already exist in org [{orgId}]!!!";

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
                r.Description = "Email or username is being by another!!!";

                return r;
            }

            user.UserStatus = "Pending";
            user.InvitedDate = DateTime.UtcNow;
            user.IsOrgInitialUser = "NO";
            user.PreviousUserStatus = "Pending";
            user.RolesList = string.Join(",", user.Roles ?? []);

            var result = repository!.AddUser(user);

            var reg = new MUserRegister()
            {
                Email = email,
                UserName = userName,
                OrgUserId = result.OrgUserId.ToString(),
                InvitedBy = user.InvitedBy,
            };
            CreateEmailUserInvitationJob(orgId, registrationCase, reg);

            r.OrgUser = result;
            //ป้องกันการ auto track กลับไปที่ column ใน table เลยต้อง assign result ให้กับ OrgUser ก่อน จากนั้นค่อยอัพเดต field อีกที
            r.OrgUser.RolesList = "";

            return r;
        }

        public MVOrganizationUser? DeleteUserById(string orgId, string userId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVOrganizationUser()
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

            var u = repository!.GetUserById(userId);
            if (u.Result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{userId}] not found for the organization [{orgId}]";

                return r;
            }

            if (u.Result.IsOrgInitialUser == "YES")
            {
                r.Status = "NOT_ALLOW_DELETE_INITIAL_USER";
                r.Description = $"Unable to delete initial user for the organization [{orgId}]";

                return r;
            }
            
            var m = repository!.DeleteUserById(userId);
            r.OrgUser = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{userId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public MOrganizationUser GetUserById(string orgId, string userId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserById(userId);

            var ou = result.Result;

            if (!string.IsNullOrEmpty(ou.RolesList))
            {
                ou.Roles = [.. ou.RolesList.Split(',')];
            }
            ou.RolesList = "";

            return ou;
        }

        public MVOrganizationUser GetUserByIdLeftJoin(string orgId, string userId)
        {
            var r = new MVOrganizationUser()
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

            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserByIdLeftJoin(userId);

            var ou = result.Result;
            if (ou == null)
            {
                r.Status = "USER_ID_NOTFOUND";
                r.Description = $"User ID [{userId}] not found in our database!!!";

                return r;
            }

            if (!string.IsNullOrEmpty(ou.RolesList))
            {
                ou.Roles = [.. ou.RolesList.Split(',')];
            }
            ou.RolesList = "";

            r.OrgUser = ou;

            return r;
        }

        public int GetUserCount(string orgId, VMOrganizationUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserCount(param);

            return result;
        }

        public int GetUserCountLeftJoin(string orgId, VMOrganizationUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserCountLeftJoin(param);

            return result;
        }

        public MVOrganizationUser? UpdateUserById(string orgId, string userId, MOrganizationUser user)
        {
            var r = new MVOrganizationUser()
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
            
            repository!.SetCustomOrgId(orgId);
            user.RolesList = string.Join(",", user.Roles ?? []);

            var result = repository!.UpdateUserById(userId, user);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{userId}] not found for the organization [{orgId}]";

                return r;
            }

            if (!string.IsNullOrEmpty(result.RolesList))
            {
                result.Roles = [.. result.RolesList.Split(',')];
            }

            r.OrgUser = result;
            //ป้องกันการ auto track กลับไปที่ column ใน table เลยต้อง assign result ให้กับ OrgUser ก่อน จากนั้นค่อยอัพเดต field อีกที
            r.OrgUser.RolesList = "";

            return r;
        }
    }
}
