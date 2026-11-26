using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPortalPageThemeRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<List<MPortalPageTheme>> GetPortalPageThemes(VMPortalPageTheme param);
        public Task<int> GetPortalPageThemeCount(VMPortalPageTheme param);
        public Task<MPortalPageTheme?> GetPortalPageThemeById(string themeId);
        public Task<MPortalPageTheme?> GetPortalPageThemeByName(string themeName);
        public Task<List<MPortalPageTheme>> GetPortalPageThemesForSelection(string authType, string language);
        public Task<MPortalPageTheme> AddPortalPageTheme(MPortalPageTheme theme);
        public Task<MPortalPageTheme?> UpdatePortalPageThemeById(string themeId, MPortalPageTheme theme);
        public Task<MPortalPageTheme?> DeletePortalPageThemeById(string themeId);
    }
}
