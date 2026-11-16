using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPortalSessionRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MPortalSession> AddPortalSession(MPortalSession portalSession);
        public Task<List<MPortalSession>> GetPortalSessions(VMPortalSession param);
        public Task<int> GetPortalSessionCount(VMPortalSession param);
        public Task<MPortalSession?> GetPortalSessionById(string portalSessionId);
        public Task<MPortalSession?> UpdatePortalSessionStatusById(string portalSessionId, string status, string? statusDesc);
    }
}
