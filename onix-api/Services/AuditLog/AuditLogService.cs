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

        public MAuditLog GetAuditLogById(string orgId, string auditLogId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetAuditLogById(auditLogId);

            return result;
        }

        public IEnumerable<MAuditLog> GetAuditLogs(string orgId, VMAuditLog param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetAuditLogs(param);

            return result;
        }

        public int GetAuditLogCount(string orgId, VMAuditLog param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetAuditLogCount(param);

            return result;
        }

        public MAuditLog GetAllAuditLogById(string auditLogId)
        {
            return repository!.GetAllAuditLogById(auditLogId);
        }

        public IEnumerable<MAuditLog> GetAllAuditLogs(VMAuditLog param)
        {
            return repository!.GetAllAuditLogs(param);
        }

        public int GetAllAuditLogCount(VMAuditLog param)
        {
            return repository!.GetAllAuditLogCount(param);
        }

        public VMAuditLogAggregations GetAllAuditLogAggregations(VMAuditLog param)
        {
            return repository!.GetAllAuditLogAggregations(param);
        }
    }
}
