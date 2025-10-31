using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IOrganizationService
    {
        public Task<MOrganization> GetOrganization(string orgId);
        public Task<MVOrganization> UpdateOrganization(string orgId, MOrganization org);
        public MVOrganizationUser AddUserToOrganization(string orgId, MOrganizationUser user);
        public bool IsUserNameExist(string orgId, string userName);
        public MVOrganizationUser VerifyUserInOrganization(string orgId, string userName);
        public MVOrganization AddOrganization(string orgId, MOrganization org);
        public IEnumerable<MOrganizationUser> GetUserAllowedOrganization(string userName);
        public bool IsOrgIdExist(string orgId);
        public MVPresignedUrl GetLogoImageUploadPresignedUrl(string orgId);
        public IEnumerable<NameValue> GetAllowChannelNames(string orgId);
        public IEnumerable<NameValue> GetAllowAddressTypeNames(string orgId);
    }
}
