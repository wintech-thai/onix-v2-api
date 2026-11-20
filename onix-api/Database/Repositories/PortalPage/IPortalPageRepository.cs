using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPortalPageRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<List<MPortalPage>> GetPortalPages(VMPortalPage param);
        public Task<int> GetPortalPageCount(VMPortalPage param);
        public Task<MPortalPage?> GetPortalPageById(string portalPageId);
        public Task<MPortalPage?> GetPortalPageByCode(string code);
        public Task<MPortalPage?> GetDefaultPortalPage();
        public Task<MPortalPage> AddPortalPage(MPortalPage portalPage);
        public Task<MPortalPage?> UpdatePortalPageById(string portalPageId, MPortalPage portalPage);
        public Task<MPortalPage?> DeletePortalPageById(string portalPageId);
        public Task<MPortalPage?> UpdatePortalPageStatusById(string portalPageId, bool isEnabled);
        public Task<MPortalPage?> SetDefaultPortalPageById(string portalPageId);
        public Task<MPortalPage?> UpdatePortalPageApiKeyById(string portalPageId, Guid apiKeyId, string apiCode);
    }
}
