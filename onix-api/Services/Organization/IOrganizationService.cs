using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IOrganizationService
    {
        public Task<MOrganization> GetOrganization(string orgId);
        public MVOrganizationUser AddUserToOrganization(string orgId, MOrganizationUser user);
        public bool IsUserNameExist(string orgId, string userName);
        public MVOrganizationUser VerifyUserInOrganization(string orgId, string userName);
        public MVOrganization AddOrganization(string orgId, MOrganization org);
        public IEnumerable<MOrganizationUser> GetUserAllowedOrganization(string userName);
    }
}
