using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PortalRealmRepository : BaseRepository, IPortalRealmRepository
    {
        public PortalRealmRepository(IDataContext ctx)
        {
            context = ctx;
        }

        //PortalPageCount/RealmUserCount เป็น correlated subquery นับจาก PortalPages/RealmUsers ของ realm นี้
        //ใช้ .Count(predicate) ตรงๆ แทน join...into สองอันซ้อนกัน เพราะ EF Core แปลเป็น SQL ไม่ได้เมื่อมี GroupJoin สองอันในการ select เดียวกัน
        private IQueryable<MPortalRealm> GetSelection(VMPortalRealm param)
        {
            var query =
                from r in context!.Realms
                select new MPortalRealm
                {
                    RealmId = r.RealmId,
                    OrgId = r.OrgId,
                    RealmName = r.RealmName,
                    AuthType = r.AuthType,
                    CreatedDate = r.CreatedDate,
                    PortalPageCount = context.PortalPages!.Count(pp => pp.RealmId == r.RealmId),
                    RealmUserCount = context.RealmUsers!.Count(ru => ru.RealmId == r.RealmId),
                };

            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                query = query.Where(x => x.RealmName!.Contains(param.FullTextSearch));
            }

            if (!string.IsNullOrEmpty(param.AuthType))
            {
                query = query.Where(x => x.AuthType!.Equals(param.AuthType));
            }

            return query;
        }

        public async Task<List<MPortalRealm>> GetRealms(VMPortalRealm param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 0;

            return await GetSelection(param)
                .OrderByDescending(x => x.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public Task<int> GetRealmCount(VMPortalRealm param)
        {
            return GetSelection(param).CountAsync();
        }

        public Task<MPortalRealm?> GetRealmById(string realmId)
        {
            var id = Guid.Parse(realmId);
            var query = context!.Realms!.Where(x => x.RealmId!.Equals(id));

            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            return query.FirstOrDefaultAsync();
        }

        public Task<MPortalRealm?> GetRealmByName(string realmName)
        {
            var query = context!.Realms!.Where(x => x.OrgId!.Equals(orgId) && x.RealmName!.Equals(realmName));
            return query.FirstOrDefaultAsync();
        }

        public async Task<MPortalRealm> AddRealm(MPortalRealm realm)
        {
            realm.OrgId = orgId;
            realm.RealmId = Guid.NewGuid();
            realm.CreatedDate = DateTime.UtcNow;

            context!.Realms!.Add(realm);
            await context.SaveChangesAsync();

            //Detach กัน caller ไปแก้ field ของ entity ตัวนี้ทีหลัง แล้วโดน save รอบถัดไปในคำขอเดียวกันเขียนทับโดยไม่ตั้งใจ (เคสเดียวกับ ApiKeyRepository)
            context.Entry(realm).State = EntityState.Detached;

            return realm;
        }

        public async Task<MPortalRealm?> UpdateRealmById(string realmId, MPortalRealm realm)
        {
            var id = Guid.Parse(realmId);
            var result = await context!.Realms!.Where(x => x.OrgId!.Equals(orgId) && x.RealmId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                //AuthType แก้ไม่ได้หลังสร้างแล้ว ตั้งใจไม่รับค่าจาก realm.AuthType ตรงนี้
                result.RealmName = realm.RealmName;

                await context!.SaveChangesAsync();

                context.Entry(result).State = EntityState.Detached;
            }

            return result;
        }

        public async Task<MPortalRealm?> DeleteRealmById(string realmId)
        {
            var id = Guid.Parse(realmId);
            var result = await context!.Realms!.Where(x => x.OrgId!.Equals(orgId) && x.RealmId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                //portal page ที่เคยผูก realm นี้ไว้ ต้องเคลียร์ RealmId ทิ้ง ไม่งั้นจะเหลือ FK ชี้ไป realm ที่ถูกลบไปแล้ว
                var pages = await context!.PortalPages!.Where(x => x.RealmId!.Equals(id)).ToListAsync();
                pages.ForEach(x => x.RealmId = null);

                context!.Realms!.Remove(result);
                await context.SaveChangesAsync();
            }

            return result;
        }

        private IQueryable<MPortalRealmUser> GetUserSelection(string realmId, VMPortalRealmUser param)
        {
            var id = Guid.Parse(realmId);
            var query = context!.RealmUsers!.Where(x => x.RealmId!.Equals(id));

            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                query = query.Where(x => x.UserName!.Contains(param.FullTextSearch) || (x.Email != null && x.Email.Contains(param.FullTextSearch)));
            }

            return query;
        }

        public async Task<List<MPortalRealmUser>> GetRealmUsers(string realmId, VMPortalRealmUser param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 0;

            return await GetUserSelection(realmId, param)
                .OrderByDescending(x => x.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public Task<int> GetRealmUserCount(string realmId, VMPortalRealmUser param)
        {
            return GetUserSelection(realmId, param).CountAsync();
        }

        public Task<MPortalRealmUser?> GetRealmUserByUserName(string realmId, string userName)
        {
            var id = Guid.Parse(realmId);
            return context!.RealmUsers!.Where(x => x.RealmId!.Equals(id) && x.UserName!.Equals(userName)).FirstOrDefaultAsync();
        }

        public async Task<MPortalRealmUser> AddRealmUser(MPortalRealmUser user)
        {
            user.OrgId = orgId;
            user.RealmUserId = Guid.NewGuid();
            user.CreatedDate = DateTime.UtcNow;

            context!.RealmUsers!.Add(user);
            await context.SaveChangesAsync();

            context.Entry(user).State = EntityState.Detached;

            return user;
        }

        public async Task<MPortalRealmUser?> ResetRealmUserPasswordById(string realmUserId, string encryptedPassword)
        {
            var id = Guid.Parse(realmUserId);
            var result = await context!.RealmUsers!.Where(x => x.OrgId!.Equals(orgId) && x.RealmUserId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.EncryptedPassword = encryptedPassword;
                await context!.SaveChangesAsync();

                context.Entry(result).State = EntityState.Detached;
            }

            return result;
        }

        public async Task<MPortalRealmUser?> UpdateRealmUserStatusById(string realmUserId, bool isEnabled)
        {
            var id = Guid.Parse(realmUserId);
            var result = await context!.RealmUsers!.Where(x => x.OrgId!.Equals(orgId) && x.RealmUserId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.IsEnabled = isEnabled;
                await context!.SaveChangesAsync();

                context.Entry(result).State = EntityState.Detached;
            }

            return result;
        }

        public async Task<MPortalRealmUser?> DeleteRealmUserById(string realmUserId)
        {
            var id = Guid.Parse(realmUserId);
            var result = await context!.RealmUsers!.Where(x => x.OrgId!.Equals(orgId) && x.RealmUserId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                context!.RealmUsers!.Remove(result);
                await context.SaveChangesAsync();
            }

            return result;
        }
    }
}
