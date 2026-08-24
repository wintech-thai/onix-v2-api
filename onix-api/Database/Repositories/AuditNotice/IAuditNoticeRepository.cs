using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAuditNoticeRepository
    {
        Task<List<MAuditNotice>> GetByRowId(string rowId);
        Task<List<MAuditNotice>> GetByOrgIdAndRowId(string orgId, string rowId);
    }
}
