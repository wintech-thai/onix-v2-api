using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class AuditLogService : BaseService, IAuditLogService
    {
        private readonly IAuditLogRepository? repository = null;

        public AuditLogService(IAuditLogRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MAuditLog> GetAuditLogById(string orgId, string auditLogId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetAuditLogById(auditLogId);
        }

        public async Task<IEnumerable<MAuditLog>> GetAuditLogs(string orgId, VMAuditLog param)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetAuditLogs(param);
        }

        public async Task<int> GetAuditLogCount(string orgId, VMAuditLog param)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetAuditLogCount(param);
        }

        public async Task<VMAuditLogAggregations> GetAuditLogAggregations(string orgId, VMAuditLog param)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetAuditLogAggregations(param);
        }

        public async Task<MAuditLog> GetAllAuditLogById(string auditLogId)
        {
            return await repository!.GetAllAuditLogById(auditLogId);
        }

        public async Task<IEnumerable<MAuditLog>> GetAllAuditLogs(VMAuditLog param)
        {
            return await repository!.GetAllAuditLogs(param);
        }

        public async Task<int> GetAllAuditLogCount(VMAuditLog param)
        {
            return await repository!.GetAllAuditLogCount(param);
        }

        public async Task<VMAuditLogAggregations> GetAllAuditLogAggregations(VMAuditLog param)
        {
            return await repository!.GetAllAuditLogAggregations(param);
        }
    }
}
