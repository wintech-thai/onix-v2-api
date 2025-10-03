using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAuditLogRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public int GetAuditLogCount(VMAuditLog param);
        public IEnumerable<MAuditLog> GetAuditLogs(VMAuditLog param);
        public MAuditLog GetAuditLogById(string logId);
    }
}
