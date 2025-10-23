using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class OrganizationUserService : BaseService, IOrganizationUserService
    {
        private readonly IOrganizationUserRepository? repository = null;
        private readonly IUserRepository? userRepository = null;
        private readonly IJobService _jobService;

        public OrganizationUserService(
            IOrganizationUserRepository repo,
            IUserRepository userRepo,
            IJobService jobService) : base()
        {
            repository = repo;
            userRepository = userRepo;
            _jobService = jobService;
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

        private MVJob? CreateEmailUserInvitationJob(string orgId, string email, string userName)
        {
            var job = new MJob()
            {
                Name = $"EmailUserInvitationJob:{Guid.NewGuid()}",
                Description = "OrgUser.CreateEmailUserInvitationJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_NOTI_ADDRESS", Value = "pjame.fb@gmail.com" },
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = "user-invitation-to-org" },
                    new NameValue { Name = "ORG_USER_NAMME", Value = userName },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                    new NameValue { Name = "REGISTRATION_URL", Value = $"https://register.please-scan.com/{orgId}/signup/will-change" },
                ]
            };

            var result = _jobService.AddJob(orgId, job);
            return result;
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

            //Validate if user exist in org
            var isUserExist = repository!.IsUserNameExist(userName);
            if (isUserExist)
            {
                r.Status = "USERNAME_DUPLICATE";
                r.Description = $"User name [{userName}] is already exist in org [{orgId}]!!!";

                return r;
            }

            user.UserStatus = "Pending";
            user.InvitedDate = DateTime.UtcNow;
            user.IsOrgInitialUser = "NO";
            user.PreviousUserStatus = "Pending";
            user.RolesList = string.Join(",", user.Roles ?? []);
            var result = repository!.AddUser(user);

            result.RolesList = "";

            CreateEmailUserInvitationJob(orgId, user.TmpUserEmail!, userName);

            r.OrgUser = result;

            return r;
        }

        public MVOrganizationUser? DeleteUserById(string orgId, string userId)
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
            result.RolesList = "";

            r.OrgUser = result;

            return r;
        }
    }
}
