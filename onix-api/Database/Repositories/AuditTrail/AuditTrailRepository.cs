using LinqKit;
using Its.Onix.Api.Models;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Database.Repositories
{
    [ExcludeFromCodeCoverage]
    public class AuditTrailRepository : BaseRepository, IAuditTrailRepository
    {
        public AuditTrailRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<List<MAuditTrack>> GetByRowId(string rowId)
        {
            return await context!.AuditTracks!
                .AsExpandable()
                .Where(p => p.RowId == rowId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
    }
}
