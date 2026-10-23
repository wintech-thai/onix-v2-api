using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAuditTrailRepository
    {
        Task<List<MAuditTrack>> GetByRowId(string rowId);

        //x088 : merchant ดู audit trail ได้เฉพาะของ org ตัวเองเท่านั้น (ต่างจาก Admin ที่ดูได้ทุก org)
        Task<List<MAuditTrack>> GetByRowId(string orgId, string rowId);
    }
}
