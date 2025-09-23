using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;

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
            //Improvement(validation) : Added validation here

            repository!.SetCustomOrgId(orgId);
            var r = new MVUser();

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

        private MVJob? CreateEmailPasswordChangeJob(string orgId, string email, string userName)
        {
            var job = new MJob()
            {
                Name = $"EmailPasswordChangeJob:{Guid.NewGuid()}",
                Description = "User.CreateEmailPasswordChangeJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = "user-password-change" },
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

            var r = _authService.ChangeUserPasswordIdp(password).Result;
            if (!r.Success)
            {
                result.Description = r.Message;
                result.Status = "IDP_UPDATE_PASSWORD_ERROR";
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
    }
}
