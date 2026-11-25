using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    //Theme เป็นของ global OrgId เสมอ ไม่ผูกกับ tenant ไหน เลยไม่ filter ด้วย orgId ตอน query เลย (ทุก tenant เห็น pool เดียวกัน)
    public class PortalPageThemeRepository : BaseRepository, IPortalPageThemeRepository
    {
        public const string GLOBAL_ORG_ID = "global";

        public PortalPageThemeRepository(IDataContext ctx)
        {
            context = ctx;
        }

        //ตอนค้นหา/list ไม่เอา HtmlTemplate มาด้วยเพราะมันใหญ่ จะช้า (ตามสเปค)
        private IQueryable<MPortalPageTheme> GetListSelection(VMPortalPageTheme param)
        {
            var query =
                from th in context!.PortalPageThemes
                select new MPortalPageTheme
                {
                    ThemeId = th.ThemeId,
                    OrgId = th.OrgId,
                    ThemeName = th.ThemeName,
                    Description = th.Description,
                    AuthType = th.AuthType,
                    Language = th.Language,
                    CreatedDate = th.CreatedDate,
                    UseCount = context.PortalPages!.Count(pp => pp.ThemeId == th.ThemeId),
                };

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                query = query.Where(x => x.ThemeName!.Contains(param.FullTextSearch));
            }

            if (!string.IsNullOrEmpty(param.AuthType))
            {
                query = query.Where(x => x.AuthType!.Equals(param.AuthType));
            }

            if (!string.IsNullOrEmpty(param.Language))
            {
                query = query.Where(x => x.Language!.Equals(param.Language));
            }

            return query;
        }

        public async Task<List<MPortalPageTheme>> GetPortalPageThemes(VMPortalPageTheme param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 0;

            return await GetListSelection(param)
                .OrderByDescending(x => x.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public Task<int> GetPortalPageThemeCount(VMPortalPageTheme param)
        {
            return GetListSelection(param).CountAsync();
        }

        public Task<MPortalPageTheme?> GetPortalPageThemeById(string themeId)
        {
            var id = Guid.Parse(themeId);
            return context!.PortalPageThemes!.Where(x => x.ThemeId!.Equals(id)).FirstOrDefaultAsync();
        }

        public Task<MPortalPageTheme?> GetPortalPageThemeByName(string themeName)
        {
            return context!.PortalPageThemes!.Where(x => x.OrgId!.Equals(GLOBAL_ORG_ID) && x.ThemeName!.Equals(themeName)).FirstOrDefaultAsync();
        }

        //สำหรับ dropdown เลือก custom theme ตอนตั้งค่า portal page - ต้อง authType+language ตรงกับของ portal page เท่านั้น
        public Task<List<MPortalPageTheme>> GetPortalPageThemesForSelection(string authType, string language)
        {
            return context!.PortalPageThemes!
                .Where(x => x.AuthType!.Equals(authType) && x.Language!.Equals(language))
                .OrderBy(x => x.ThemeName)
                .ToListAsync();
        }

        public async Task<MPortalPageTheme> AddPortalPageTheme(MPortalPageTheme theme)
        {
            theme.OrgId = GLOBAL_ORG_ID;
            theme.ThemeId = Guid.NewGuid();
            theme.CreatedDate = DateTime.UtcNow;

            context!.PortalPageThemes!.Add(theme);
            await context.SaveChangesAsync();

            context.Entry(theme).State = EntityState.Detached;

            return theme;
        }

        public async Task<MPortalPageTheme?> UpdatePortalPageThemeById(string themeId, MPortalPageTheme theme)
        {
            var id = Guid.Parse(themeId);
            var result = await context!.PortalPageThemes!.Where(x => x.ThemeId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                //AuthType ล็อกตั้งแต่สร้าง แก้ไม่ได้ ตามสเปค
                result.ThemeName = theme.ThemeName;
                result.Description = theme.Description;
                result.Language = theme.Language;
                result.VariablesJson = theme.VariablesJson;
                result.HtmlTemplate = theme.HtmlTemplate;

                await context!.SaveChangesAsync();

                context.Entry(result).State = EntityState.Detached;
            }

            return result;
        }

        public async Task<MPortalPageTheme?> DeletePortalPageThemeById(string themeId)
        {
            var id = Guid.Parse(themeId);
            var result = await context!.PortalPageThemes!.Where(x => x.ThemeId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                //portal page ที่เคยผูก theme นี้ไว้ ต้องเคลียร์ ThemeId ทิ้ง ไม่งั้นจะเหลือ FK ชี้ไป theme ที่ถูกลบไปแล้ว
                var pages = await context!.PortalPages!.Where(x => x.ThemeId!.Equals(id)).ToListAsync();
                pages.ForEach(x => x.ThemeId = null);

                context!.PortalPageThemes!.Remove(result);
                await context.SaveChangesAsync();
            }

            return result;
        }
    }
}
