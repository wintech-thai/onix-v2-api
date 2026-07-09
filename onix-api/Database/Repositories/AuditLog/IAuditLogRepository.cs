using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAuditLogRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<int> GetAuditLogCount(VMAuditLog param);
        public Task<IEnumerable<MAuditLog>> GetAuditLogs(VMAuditLog param);
        public Task<MAuditLog> GetAuditLogById(string logId);
        public Task<VMAuditLogAggregations> GetAuditLogAggregations(VMAuditLog param);
        public Task<int> GetAllAuditLogCount(VMAuditLog param);
        public Task<IEnumerable<MAuditLog>> GetAllAuditLogs(VMAuditLog param);
        public Task<MAuditLog> GetAllAuditLogById(string logId);
        public Task<VMAuditLogAggregations> GetAllAuditLogAggregations(VMAuditLog param);
    }
}
