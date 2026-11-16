using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPortalSessionService
    {
        public Task<MVPortalSession> GetPortalSessions(string orgId, VMPortalSession param);
        public Task<int> GetPortalSessionCount(string orgId, VMPortalSession param);
        public Task<MVPortalSession> GetPortalSessionById(string orgId, string portalSessionId);
        public Task<MVPortalSession> UpdatePortalSessionStatusById(string orgId, string portalSessionId, string status, string? statusDesc);

        //Public (ไม่ผ่าน auth token) - เรียกจากหน้า portal page จริง ๆ เวลา Mikrotik/ผู้ใช้เปิดเข้ามา
        public Task<MVPortalSession> AddPortalSessionPublic(string orgId, string? code, string? apiKeyValue, Dictionary<string, string> parameters);
    }
}
