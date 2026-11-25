using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPortalPageThemeService
    {
        public Task<IEnumerable<MPortalPageTheme>> GetPortalPageThemes(VMPortalPageTheme param);
        public Task<int> GetPortalPageThemeCount(VMPortalPageTheme param);
        public Task<MVPortalPageTheme> GetPortalPageThemeById(string themeId);
        public Task<IEnumerable<MPortalPageTheme>> GetPortalPageThemesForSelection(string authType, string language);
        public Task<MVPortalPageTheme> AddPortalPageTheme(MPortalPageTheme theme);
        public Task<MVPortalPageTheme> UpdatePortalPageThemeById(string themeId, MPortalPageTheme theme);
        public Task<MVPortalPageTheme> DeletePortalPageThemeById(string themeId);
    }
}
