using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAuditTrailRepository
    {
        Task<List<MAuditTrack>> GetByRowId(string rowId);
    }
}
