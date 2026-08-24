using LinqKit;
using Its.Onix.Api.Models;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Database.Repositories
{
    [ExcludeFromCodeCoverage]
    public class AuditNoticeRepository : BaseRepository, IAuditNoticeRepository
    {
        public AuditNoticeRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<List<MAuditNotice>> GetByRowId(string rowId)
        {
            return await context!.AuditNotices!
                .AsExpandable()
                .Where(p => p.RowId == rowId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<MAuditNotice>> GetByOrgIdAndRowId(string orgId, string rowId)
        {
            return await context!.AuditNotices!
                .AsExpandable()
                .Where(p => p.OrgId == orgId && p.RowId == rowId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
    }
}
