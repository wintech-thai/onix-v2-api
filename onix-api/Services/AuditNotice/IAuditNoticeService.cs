using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public interface IAuditNoticeService
    {
        Task<List<MAuditNotice>> GetByRowId(string rowId);
        Task<List<MAuditNotice>> GetByOrgIdAndRowId(string orgId, string rowId);
    }
}
