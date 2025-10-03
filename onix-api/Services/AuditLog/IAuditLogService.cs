using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IAuditLogService
    {
        public MAuditLog GetAuditLogById(string orgId, string auditLogId);
        public IEnumerable<MAuditLog> GetAuditLogs(string orgId, VMAuditLog param);
        public int GetAuditLogCount(string orgId, VMAuditLog param);
    }
}
