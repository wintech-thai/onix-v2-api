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

        //เรียกจากหน้า portal page จริง ๆ เวลา Mikrotik/ผู้ใช้เปิดเข้ามา - OrgUser-level route, auth ด้วย api key ผ่าน Basic auth
        public Task<MVPortalSession> AddPortalSession(string orgId, string? code, string? apiKeyId, Dictionary<string, string> parameters);
    }
}
