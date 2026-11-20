using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPortalPageService
    {
        public Task<IEnumerable<MPortalPage>> GetPortalPages(string orgId, VMPortalPage param);
        public Task<int> GetPortalPageCount(string orgId, VMPortalPage param);
        public Task<MVPortalPage> GetPortalPageById(string orgId, string portalPageId);
        public Task<MVPortalPage> AddPortalPage(string orgId, MPortalPage portalPage);
        public Task<MVPortalPage> UpdatePortalPageById(string orgId, string portalPageId, MPortalPage portalPage);
        public Task<MVPortalPage> DeletePortalPageById(string orgId, string portalPageId);
        public Task<MVPortalPage> UpdatePortalPageStatusById(string orgId, string portalPageId, bool isEnabled);
        public Task<MVPortalPage> SetDefaultPortalPageById(string orgId, string portalPageId);
        public Task<MVApiKey> CreatePortalPageApiKey(string orgId, string portalPageId, string keyName, string? keyDescription);

        //คืน api key เดิมของ portal page ถ้ามีอยู่แล้ว ไม่สร้างใหม่ทับ (สร้างใหม่เฉพาะตอนยังไม่เคยมี)
        public Task<MVApiKey> GetPortalPageApiKey(string orgId, string portalPageId, string keyName, string? keyDescription);

        //ใช้โดย public controller (Mikrotik/หน้า portal) ไม่ผ่าน auth token ปกติ
        public Task<MPortalPage?> GetPortalPageByCodeForPublic(string orgId, string code);
        public Task<MPortalPage?> GetDefaultPortalPageForPublic(string orgId);
    }
}
