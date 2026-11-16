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

        private IQueryable<MPortalPage> GetSelection(VMPortalPage param)
        {
            var query = context!.PortalPages!.AsQueryable();

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
            var query = context!.PortalPages!.Where(x => x.PortalPageId!.Equals(id));

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

                await context!.SaveChangesAsync();
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
