using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Models;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Services
{
    [ExcludeFromCodeCoverage]
    public class AuditNoticeService : BaseService, IAuditNoticeService
    {
        private readonly IAuditNoticeRepository repository;

        public AuditNoticeService(IAuditNoticeRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<List<MAuditNotice>> GetByRowId(string rowId)
        {
            return await repository.GetByRowId(rowId);
        }

        public async Task<List<MAuditNotice>> GetByOrgIdAndRowId(string orgId, string rowId)
        {
            return await repository.GetByOrgIdAndRowId(orgId, rowId);
        }
    }
}
