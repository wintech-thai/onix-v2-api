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

        //เรียกจากหน้า login portal (Native) ตอนผู้ใช้กรอก user/password - เช็คกับ realm user ของ portal page นี้ แล้วอัปเดต session
        public Task<MVPortalSession> VerifyNativeLogin(string orgId, string portalSessionId, string userName, string password);

        //กดปุ่ม "Login with Google" - ส่งแค่ domain มา
        public Task<MVPortalSession> InitiateGoogleLogin(string orgId, string portalSessionId, string domain);

        //Google redirect กลับมาเรียกตรงนี้ (public)
        public Task<MVPortalSession> HandleGoogleCallback(string orgId, string? code, string? state, string? error);
    }
}
