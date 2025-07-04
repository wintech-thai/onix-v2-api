using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IOrganizationUserService
    {
        public MVOrganizationUser? AddUser(string orgId, MOrganizationUser user);
        public MVOrganizationUser? DeleteUserById(string orgId, string userId);
        public IEnumerable<MOrganizationUser> GetUsers(string orgId, VMOrganizationUser param);
        public int GetUserCount(string orgId, VMOrganizationUser param);
        public MVOrganizationUser? UpdateUserById(string orgId, string userId, MOrganizationUser user);
        public MOrganizationUser GetUserById(string orgId, string userId);
    }
}
