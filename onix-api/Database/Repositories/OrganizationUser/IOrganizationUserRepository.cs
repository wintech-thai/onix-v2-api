using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IOrganizationUserRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MOrganizationUser> GetUserById(string orgUserId);
        public MOrganizationUser AddUser(MOrganizationUser user);
        public MOrganizationUser? DeleteUserById(string orgUserId);
        public IEnumerable<MOrganizationUser> GetUsers(VMOrganizationUser param);
        public int GetUserCount(VMOrganizationUser param);
        public MOrganizationUser? UpdateUserById(string orgUserId, MOrganizationUser user);
    }
}
