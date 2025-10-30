using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserRepository repository;
        private readonly IAuthService _authService;
        private readonly IJobService _jobService;

        public UserService(
            IUserRepository repo,
            IJobService jobService,
            IAuthService authService) : base()
        {
            repository = repo;
            _authService = authService;
            _jobService = jobService;
        }

        public MVUser AddUser(string orgId, MUser user)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVUser();

            var userValidateResult = ValidationUtils.ValidateUserName(user.UserName!);
            if (userValidateResult.Status != "OK")
            {
                r.Status = userValidateResult.Status;
                r.Description = userValidateResult.Description;

                return r;
            }

            var emailValidateResult = ValidationUtils.ValidateEmail(user.UserEmail!);
            if (emailValidateResult.Status != "OK")
            {
                r.Status = emailValidateResult.Status;
                r.Description = emailValidateResult.Description;

                return r;
            }

            var f1 = IsEmailExist(orgId, user!.UserEmail!);
            var f2 = IsUserNameExist(orgId, user!.UserName!);

            if (f1)
            {
                r.Status = "EMAIL_DUPLICATE";
                r.Description = $"Email is [{user!.UserEmail!}] duplicate";

                return r;
            }

            if (f2)
            {
                r.Status = "USERNAME_DUPLICATE";
                r.Description = $"User name [{user!.UserName!}] is duplicate";

                return r;
            }

            var result = repository!.AddUser(user);

            r.Status = "OK";
            r.Description = "Success";
            r.User = result;

            return r;
        }

        public IEnumerable<MUser> GetUsers(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUsers();

            return result;
        }

        public bool IsEmailExist(string orgId, string email)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.IsEmailExist(email);

            return result;
        }

        public bool IsUserNameExist(string orgId, string userName)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.IsUserNameExist(userName);

            return result;
        }

        public bool IsUserIdExist(string orgId, string userId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.IsUserIdExist(userId);

            return result;
        }

        public MUser GetUserByName(string orgId, string userName)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserByName(userName);

            return result;
        }

        public MUser GetUserByEmail(string orgId, string email)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserByEmail(email);

            return result;
        }

        public MVJob? CreateEmailPasswordChangeJob(string orgId, string email, string userName)
        {
            var templateType = "user-password-change";
            var job = new MJob()
            {
                Name = $"EmailPasswordChangeJob:{Guid.NewGuid()}",
                Description = "User.CreateEmailPasswordChangeJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",
                Tags = templateType,

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new NameValue { Name = "ORG_USER_NAMME", Value = userName },
                ]
            };

            var result = _jobService.AddJob(orgId, job);
            return result;
        }

        public MVUpdatePassword UpdatePassword(string userName, MUpdatePassword password)
        {
            var result = new MVUpdatePassword()
            {
                Status = "SUCCESS",
                Description = $"Updated password for user [{userName}]",
            };

            var validateResult = ValidationUtils.ValidatePassword(password.NewPassword);
            if (validateResult.Status != "OK")
            {
                result.Status = validateResult.Status;
                result.Description = validateResult.Description;

                return result;
            }

            password.UserName = userName;
            var r = _authService.ChangeUserPasswordIdp(password).Result;
            if (!r.Success)
            {
                result.Description = r.Message;
                result.Status = "IDP_UPDATE_PASSWORD_ERROR";

                return result;
            }

            //Send email to user to notify password change
            var user = GetUserByName("notuse", userName);
            if (user == null)
            {
                result.Status = "USERNAME_NOT_FOUND_DB";
                result.Description = $"Unable to find user name [{userName}] in DB!!!";

                return result;
            }

            CreateEmailPasswordChangeJob("notuse", user.UserEmail!, userName);

            return result;
        }

        public MVLogout UserLogout(string userName)
        {
            var result = new MVLogout()
            {
                Status = "SUCCESS",
                Description = $"Logout for user [{userName}]",
            };

            var r = _authService.UserLogoutIdp(userName).Result;
            if (!r.Success)
            {
                result.Description = r.Message;
                result.Status = "IDP_LOGOUT_ERROR";
            }

            return result;
        }
    }
}
