using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IAuditLogService
    {
        public Task<MAuditLog> GetAuditLogById(string orgId, string auditLogId);
        public Task<IEnumerable<MAuditLog>> GetAuditLogs(string orgId, VMAuditLog param);
        public Task<int> GetAuditLogCount(string orgId, VMAuditLog param);
        public Task<MAuditLog> GetAllAuditLogById(string auditLogId);
        public Task<IEnumerable<MAuditLog>> GetAllAuditLogs(VMAuditLog param);
        public Task<int> GetAllAuditLogCount(VMAuditLog param);
        public Task<VMAuditLogAggregations> GetAllAuditLogAggregations(VMAuditLog param);
    }
}
