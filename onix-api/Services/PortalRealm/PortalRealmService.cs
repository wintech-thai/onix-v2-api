using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class PortalRealmService : BaseService, IPortalRealmService
    {
        private readonly IPortalRealmRepository? repository = null;

        public PortalRealmService(IPortalRealmRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<IEnumerable<MPortalRealm>> GetRealms(string orgId, VMPortalRealm param)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetRealms(param);
        }

        public Task<int> GetRealmCount(string orgId, VMPortalRealm param)
        {
            repository!.SetCustomOrgId(orgId);
            return repository!.GetRealmCount(param);
        }

        public async Task<MVPortalRealm> GetRealmById(string orgId, string realmId)
        {
            var r = new MVPortalRealm() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetRealmById(realmId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm ID [{realmId}] not found";
                return r;
            }

            r.Realm = result;
            return r;
        }

        public async Task<MVPortalRealm> AddRealm(string orgId, MPortalRealm realm)
        {
            var r = new MVPortalRealm() { Status = "OK", Description = "Success" };

            if (string.IsNullOrWhiteSpace(realm.RealmName))
            {
                r.Status = "ERROR_VALIDATION_REALM_NAME";
                r.Description = "Realm name is required";
                return r;
            }

            //เบื้องต้นรองรับแค่ Native ก่อนตามสเปค ฝั่ง frontend disable ตัวเลือก Google ไว้แล้ว เช็คซ้ำฝั่ง backend กันเรียก API ตรงข้าม UI
            if (realm.AuthType != "Native")
            {
                r.Status = "ERROR_AUTH_TYPE_NOT_SUPPORTED";
                r.Description = "Only Native auth type is supported for now";
                return r;
            }

            repository!.SetCustomOrgId(orgId);

            var existing = await repository!.GetRealmByName(realm.RealmName!);
            if (existing != null)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Realm name [{realm.RealmName}] is already used in this organization";
                return r;
            }

            var result = await repository!.AddRealm(realm);

            r.Realm = result;
            return r;
        }

        public async Task<MVPortalRealm> UpdateRealmById(string orgId, string realmId, MPortalRealm realm)
        {
            var r = new MVPortalRealm() { Status = "OK", Description = "Success" };

            if (string.IsNullOrWhiteSpace(realm.RealmName))
            {
                r.Status = "ERROR_VALIDATION_REALM_NAME";
                r.Description = "Realm name is required";
                return r;
            }

            repository!.SetCustomOrgId(orgId);

            var existing = await repository!.GetRealmByName(realm.RealmName!);
            if (existing != null && existing.RealmId.ToString() != realmId)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Realm name [{realm.RealmName}] is already used in this organization";
                return r;
            }

            var result = await repository!.UpdateRealmById(realmId, realm);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm ID [{realmId}] not found";
                return r;
            }

            r.Realm = result;
            return r;
        }

        public async Task<MVPortalRealm> DeleteRealmById(string orgId, string realmId)
        {
            var r = new MVPortalRealm() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.DeleteRealmById(realmId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm ID [{realmId}] not found";
                return r;
            }

            r.Realm = result;
            return r;
        }

        public async Task<IEnumerable<MPortalRealmUser>> GetRealmUsers(string orgId, string realmId, VMPortalRealmUser param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetRealmUsers(realmId, param);

            //ไม่ส่ง hash password กลับไปฝั่ง frontend
            result.ForEach(x => x.EncryptedPassword = null);
            return result;
        }

        public Task<int> GetRealmUserCount(string orgId, string realmId, VMPortalRealmUser param)
        {
            repository!.SetCustomOrgId(orgId);
            return repository!.GetRealmUserCount(realmId, param);
        }

        public async Task<MVPortalRealmUser> GetRealmUserById(string orgId, string realmUserId)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetRealmUserById(realmUserId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm User ID [{realmUserId}] not found";
                return r;
            }

            result.EncryptedPassword = null;
            r.RealmUser = result;
            return r;
        }

        public async Task<MVPortalRealmUser> AddRealmUser(string orgId, string realmId, MPortalRealmUser user)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                r.Status = "ERROR_VALIDATION_USERNAME";
                r.Description = "Username is required";
                return r;
            }

            var pwdValidation = ValidationUtils.ValidatePassword(user.Password ?? "");
            if (pwdValidation.Status != "OK")
            {
                r.Status = pwdValidation.Status!;
                r.Description = pwdValidation.Description!;
                return r;
            }

            repository!.SetCustomOrgId(orgId);

            var realm = await repository!.GetRealmById(realmId);
            if (realm == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm ID [{realmId}] not found";
                return r;
            }

            var existing = await repository!.GetRealmUserByUserName(realmId, user.UserName!);
            if (existing != null)
            {
                r.Status = "USERNAME_DUPLICATE";
                r.Description = $"Username [{user.UserName}] already exists in this realm";
                return r;
            }

            user.RealmId = realm.RealmId;
            user.EncryptedPassword = PasswordHashUtils.HashPassword(user.Password!);

            var result = await repository!.AddRealmUser(user);
            result.EncryptedPassword = null;

            r.RealmUser = result;
            return r;
        }

        public async Task<MVPortalRealmUser> UpdateRealmUserById(string orgId, string realmUserId, MPortalRealmUser user)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.UpdateRealmUserById(realmUserId, user);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm User ID [{realmUserId}] not found";
                return r;
            }

            result.EncryptedPassword = null;
            r.RealmUser = result;
            return r;
        }

        public async Task<MVPortalRealmUser> ResetRealmUserPasswordById(string orgId, string realmUserId, string password)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            var pwdValidation = ValidationUtils.ValidatePassword(password);
            if (pwdValidation.Status != "OK")
            {
                r.Status = pwdValidation.Status!;
                r.Description = pwdValidation.Description!;
                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var encrypted = PasswordHashUtils.HashPassword(password);
            var result = await repository!.ResetRealmUserPasswordById(realmUserId, encrypted);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm User ID [{realmUserId}] not found";
                return r;
            }

            result.EncryptedPassword = null;
            r.RealmUser = result;
            return r;
        }

        public async Task<MVPortalRealmUser> UpdateRealmUserStatusById(string orgId, string realmUserId, bool isEnabled)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.UpdateRealmUserStatusById(realmUserId, isEnabled);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm User ID [{realmUserId}] not found";
                return r;
            }

            result.EncryptedPassword = null;
            r.RealmUser = result;
            return r;
        }

        public async Task<MVPortalRealmUser> VerifyRealmUserLogin(string orgId, string realmId, string userName, string password)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var user = await repository!.GetRealmUserByUserName(realmId, userName);

            //เหตุผลเดียวกันทั้งไม่เจอ user และ password ผิด กันคนเดาว่า username นี้มีอยู่จริงไหม
            var wrongCredsDesc = "Username or password is incorrect";

            if (user == null)
            {
                r.Status = "INVALID_CREDENTIALS";
                r.Description = wrongCredsDesc;
                return r;
            }

            if (user.IsEnabled != true)
            {
                r.Status = "USER_DISABLED";
                r.Description = "This user is disabled";
                return r;
            }

            if (!PasswordHashUtils.VerifyPassword(user.EncryptedPassword ?? "", password))
            {
                r.Status = "INVALID_CREDENTIALS";
                r.Description = wrongCredsDesc;
                return r;
            }

            var updated = await repository!.UpdateRealmUserLastSeenById(user.RealmUserId!.Value.ToString());

            r.RealmUser = updated ?? user;
            r.RealmUser.EncryptedPassword = null;
            return r;
        }

        public async Task<MVPortalRealmUser> DeleteRealmUserById(string orgId, string realmUserId)
        {
            var r = new MVPortalRealmUser() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.DeleteRealmUserById(realmUserId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Realm User ID [{realmUserId}] not found";
                return r;
            }

            r.RealmUser = result;
            return r;
        }
    }
}
