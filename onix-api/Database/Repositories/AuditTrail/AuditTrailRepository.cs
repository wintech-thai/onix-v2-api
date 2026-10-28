using Its.Onix.Api.Models;
using Microsoft.EntityFrameworkCore;
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
                .Where(p => p.RowId == rowId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        // x089 : audit rows are stamped with the OrgId of whoever acted (could be Admin
        // acting on behalf of a MerchantOwn merchant), not the OrgId of the record itself —
        // so filtering audit rows by actor OrgId hid entries for any MerchantOwn merchant
        // whose record an admin had touched. Ownership must instead be checked against the
        // target row's own OrgId on PaymentRequests/PaymentTransactions (the 2 audited
        // tables); once confirmed, every audit entry for that row is returned regardless of
        // which org the acting user was in.
        public async Task<List<MAuditTrack>> GetByRowId(string orgId, string rowId)
        {
            if (!Guid.TryParse(rowId, out var id))
            {
                return new List<MAuditTrack>();
            }

            var ownsRow = await context!.PaymentRequests!.AnyAsync(p => p.Id == id && p.OrgId == orgId)
                || await context!.PaymentTransactions!.AnyAsync(p => p.Id == id && p.OrgId == orgId);

            if (!ownsRow)
            {
                return new List<MAuditTrack>();
            }

            return await context!.AuditTracks!
                .Where(p => p.RowId == rowId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
    }
}
