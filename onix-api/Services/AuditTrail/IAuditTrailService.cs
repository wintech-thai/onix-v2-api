using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public interface IAuditTrailService
    {
        Task<List<MAuditTrack>> GetByRowId(string rowId);
    }
}
