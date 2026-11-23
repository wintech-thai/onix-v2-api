using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPortalRealmService
    {
        public Task<IEnumerable<MPortalRealm>> GetRealms(string orgId, VMPortalRealm param);
        public Task<int> GetRealmCount(string orgId, VMPortalRealm param);
        public Task<MVPortalRealm> GetRealmById(string orgId, string realmId);
        public Task<MVPortalRealm> AddRealm(string orgId, MPortalRealm realm);
        public Task<MVPortalRealm> UpdateRealmById(string orgId, string realmId, MPortalRealm realm);
        public Task<MVPortalRealm> DeleteRealmById(string orgId, string realmId);

        public Task<IEnumerable<MPortalRealmUser>> GetRealmUsers(string orgId, string realmId, VMPortalRealmUser param);
        public Task<int> GetRealmUserCount(string orgId, string realmId, VMPortalRealmUser param);
        public Task<MVPortalRealmUser> GetRealmUserById(string orgId, string realmUserId);
        public Task<MVPortalRealmUser> AddRealmUser(string orgId, string realmId, MPortalRealmUser user);
        public Task<MVPortalRealmUser> UpdateRealmUserById(string orgId, string realmUserId, MPortalRealmUser user);
        public Task<MVPortalRealmUser> ResetRealmUserPasswordById(string orgId, string realmUserId, string password);
        public Task<MVPortalRealmUser> UpdateRealmUserStatusById(string orgId, string realmUserId, bool isEnabled);

        //ใช้จากหน้า login portal (Native) เช็ค username/password ของ realm user แล้วอัปเดต last seen ถ้าถูก
        public Task<MVPortalRealmUser> VerifyRealmUserLogin(string orgId, string realmId, string userName, string password);
        public Task<MVPortalRealmUser> DeleteRealmUserById(string orgId, string realmUserId);
    }
}
