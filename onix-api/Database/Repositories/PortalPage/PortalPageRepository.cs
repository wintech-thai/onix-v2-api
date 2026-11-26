using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PortalPageRepository : BaseRepository, IPortalPageRepository
    {
        public PortalPageRepository(IDataContext ctx)
        {
            context = ctx;
        }

        //เอา RealmName มาจาก left outer join กับ PortalRealms เสมอ (ไม่ denormalize) เปลี่ยนชื่อ realm แล้วตรงนี้เปลี่ยนตามทันที
        private IQueryable<MPortalPage> GetJoinedSelection()
        {
            var query =
                from pp in context!.PortalPages
                join r in context.Realms! on pp.RealmId equals r.RealmId into realmJoin
                from realm in realmJoin.DefaultIfEmpty()
                join th in context.PortalPageThemes! on pp.ThemeId equals th.ThemeId into themeJoin
                from theme in themeJoin.DefaultIfEmpty()
                select new MPortalPage
                {
                    PortalPageId = pp.PortalPageId,
                    OrgId = pp.OrgId,
                    Code = pp.Code,
                    AuthMethod = pp.AuthMethod,
                    Theme = pp.Theme,
                    ThemeId = pp.ThemeId,
                    ThemeName = theme != null ? theme.ThemeName : null,
                    IsDefault = pp.IsDefault,
                    IsEnabled = pp.IsEnabled,
                    ApiKeyId = pp.ApiKeyId,
                    ApiCode = pp.ApiCode,
                    RealmId = pp.RealmId,
                    RealmName = realm != null ? realm.RealmName : null,
                    Language = pp.Language ?? "Thai", //ของเก่า null/empty ให้มองเป็น Thai เสมอ
                    RedirectUrlMode = pp.RedirectUrlMode,
                    RedirectUrlValue = pp.RedirectUrlValue,
                    CreatedDate = pp.CreatedDate,
                };

            return query;
        }

        private IQueryable<MPortalPage> GetSelection(VMPortalPage param)
        {
            var query = GetJoinedSelection();

            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                query = query.Where(x => x.Code!.Contains(param.FullTextSearch));
            }

            if (!string.IsNullOrEmpty(param.AuthMethod))
            {
                query = query.Where(x => x.AuthMethod!.Equals(param.AuthMethod));
            }

            if (!string.IsNullOrEmpty(param.Status))
            {
                var wantEnabled = param.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(x => x.IsEnabled == wantEnabled);
            }

            return query;
        }

        public async Task<List<MPortalPage>> GetPortalPages(VMPortalPage param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 0;

            var result = await GetSelection(param)
                .OrderByDescending(x => x.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return result;
        }

        public Task<int> GetPortalPageCount(VMPortalPage param)
        {
            return GetSelection(param).CountAsync();
        }

        public Task<MPortalPage?> GetPortalPageById(string portalPageId)
        {
            var id = Guid.Parse(portalPageId);
            var query = GetJoinedSelection().Where(x => x.PortalPageId!.Equals(id));

            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            return query.FirstOrDefaultAsync();
        }

        public Task<MPortalPage?> GetPortalPageByCode(string code)
        {
            var query = context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.Code!.Equals(code));
            return query.FirstOrDefaultAsync();
        }

        public Task<MPortalPage?> GetDefaultPortalPage()
        {
            var query = context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.IsDefault == true);
            return query.FirstOrDefaultAsync();
        }

        public async Task<MPortalPage> AddPortalPage(MPortalPage portalPage)
        {
            portalPage.OrgId = orgId;
            portalPage.PortalPageId = Guid.NewGuid();
            portalPage.CreatedDate = DateTime.UtcNow;

            context!.PortalPages!.Add(portalPage);
            await context.SaveChangesAsync();

            //Detach กัน caller ไปแก้ field ของ entity ตัวนี้ทีหลัง แล้วโดน save รอบถัดไปในคำขอเดียวกันเขียนทับโดยไม่ตั้งใจ (เคสเดียวกับ ApiKeyRepository)
            context.Entry(portalPage).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            if (portalPage.RealmId != null)
            {
                var realm = await context!.Realms!.Where(x => x.RealmId!.Equals(portalPage.RealmId)).FirstOrDefaultAsync();
                portalPage.RealmName = realm?.RealmName;
            }

            if (portalPage.ThemeId != null)
            {
                var theme = await context!.PortalPageThemes!.Where(x => x.ThemeId!.Equals(portalPage.ThemeId)).FirstOrDefaultAsync();
                portalPage.ThemeName = theme?.ThemeName;
            }

            return portalPage;
        }

        public async Task<MPortalPage?> UpdatePortalPageById(string portalPageId, MPortalPage portalPage)
        {
            var id = Guid.Parse(portalPageId);
            var result = await context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.PortalPageId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Code = portalPage.Code;
                result.AuthMethod = portalPage.AuthMethod;
                result.Theme = portalPage.Theme;
                result.ThemeId = portalPage.ThemeId;
                result.RealmId = portalPage.RealmId;
                result.Language = portalPage.Language;
                result.RedirectUrlMode = portalPage.RedirectUrlMode;
                result.RedirectUrlValue = portalPage.RedirectUrlValue;

                await context!.SaveChangesAsync();

                //Detach ด้วยเหตุผลเดียวกับ ApiKeyRepository/AddPortalPage
                context.Entry(result).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

                if (result.RealmId != null)
                {
                    var realm = await context!.Realms!.Where(x => x.RealmId!.Equals(result.RealmId)).FirstOrDefaultAsync();
                    result.RealmName = realm?.RealmName;
                }
                else
                {
                    result.RealmName = null;
                }

                if (result.ThemeId != null)
                {
                    var theme = await context!.PortalPageThemes!.Where(x => x.ThemeId!.Equals(result.ThemeId)).FirstOrDefaultAsync();
                    result.ThemeName = theme?.ThemeName;
                }
                else
                {
                    result.ThemeName = null;
                }
            }

            return result;
        }

        public async Task<MPortalPage?> DeletePortalPageById(string portalPageId)
        {
            var id = Guid.Parse(portalPageId);
            var result = await context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.PortalPageId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                context!.PortalPages!.Remove(result);
                await context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<MPortalPage?> UpdatePortalPageStatusById(string portalPageId, bool isEnabled)
        {
            var id = Guid.Parse(portalPageId);
            var result = await context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.PortalPageId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.IsEnabled = isEnabled;
                await context!.SaveChangesAsync();
            }

            return result;
        }

        public async Task<MPortalPage?> SetDefaultPortalPageById(string portalPageId)
        {
            var id = Guid.Parse(portalPageId);
            var target = await context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.PortalPageId!.Equals(id)).FirstOrDefaultAsync();

            if (target == null)
            {
                return null;
            }

            //default มีได้แค่อันเดียวต่อ org เลยต้องเคลียร์ตัวเดิมก่อน
            var others = await context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.IsDefault == true).ToListAsync();
            others.ForEach(x => x.IsDefault = false);

            target.IsDefault = true;
            await context!.SaveChangesAsync();

            return target;
        }

        public async Task<MPortalPage?> UpdatePortalPageApiKeyById(string portalPageId, Guid apiKeyId, string apiCode)
        {
            var id = Guid.Parse(portalPageId);
            var result = await context!.PortalPages!.Where(x => x.OrgId!.Equals(orgId) && x.PortalPageId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.ApiKeyId = apiKeyId;
                result.ApiCode = apiCode;
                await context!.SaveChangesAsync();
            }

            return result;
        }
    }
}
