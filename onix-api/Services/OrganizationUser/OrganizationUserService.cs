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

        public OrganizationUserService(IOrganizationUserRepository repo, IUserRepository userRepo) : base()
        {
            repository = repo;
            userRepository = userRepo;
        }

        public IEnumerable<MOrganizationUser> GetUsers(string orgId, VMOrganizationUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUsers(param);

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

            return result.Result;
        }

        public int GetUserCount(string orgId, VMOrganizationUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetUserCount(param);

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
            var result = repository!.UpdateUserById(userId, user);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"User ID [{userId}] not found for the organization [{orgId}]";

                return r;
            }

            r.OrgUser = result;
            return r;
        }
    }
}
