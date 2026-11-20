using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPortalRealmRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<List<MPortalRealm>> GetRealms(VMPortalRealm param);
        public Task<int> GetRealmCount(VMPortalRealm param);
        public Task<MPortalRealm?> GetRealmById(string realmId);
        public Task<MPortalRealm?> GetRealmByName(string realmName);
        public Task<MPortalRealm> AddRealm(MPortalRealm realm);
        public Task<MPortalRealm?> UpdateRealmById(string realmId, MPortalRealm realm);
        public Task<MPortalRealm?> DeleteRealmById(string realmId);

        public Task<List<MPortalRealmUser>> GetRealmUsers(string realmId, VMPortalRealmUser param);
        public Task<int> GetRealmUserCount(string realmId, VMPortalRealmUser param);
        public Task<MPortalRealmUser?> GetRealmUserByUserName(string realmId, string userName);
        public Task<MPortalRealmUser> AddRealmUser(MPortalRealmUser user);
        public Task<MPortalRealmUser?> ResetRealmUserPasswordById(string realmUserId, string encryptedPassword);
        public Task<MPortalRealmUser?> UpdateRealmUserStatusById(string realmUserId, bool isEnabled);
        public Task<MPortalRealmUser?> DeleteRealmUserById(string realmUserId);
    }
}
