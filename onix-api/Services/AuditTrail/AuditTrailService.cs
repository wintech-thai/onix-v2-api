using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Models;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Services
{
    [ExcludeFromCodeCoverage]
    public class AuditTrailService : BaseService, IAuditTrailService
    {
        private readonly IAuditTrailRepository repository;

        public AuditTrailService(IAuditTrailRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<List<MAuditTrack>> GetByRowId(string rowId)
        {
            return await repository.GetByRowId(rowId);
        }

        //x088 : merchant ดู audit trail ได้เฉพาะของ org ตัวเองเท่านั้น (ต่างจาก Admin ที่ดูได้ทุก org)
        public async Task<List<MAuditTrack>> GetByRowId(string orgId, string rowId)
        {
            return await repository.GetByRowId(orgId, rowId);
        }
    }
}
