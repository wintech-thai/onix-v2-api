using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Database.Repositories
{
    [ExcludeFromCodeCoverage]
    public class CaseManagementRepository : BaseRepository, ICaseManagementRepository
    {
        private const string RESERVE_ORG_ID = "axxxxnotdefinedxxxxxxa";

        public CaseManagementRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MCaseManagement?> GetCaseById(string caseId)
        {
            Guid id = Guid.Parse(caseId);
            return await context!.CaseManagements!
                .AsExpandable()
                .Where(p => p.Id!.Equals(id))
                .FirstOrDefaultAsync();
        }

        public async Task<List<MCaseManagement>> GetCases(VMCaseManagement param)
        {
            var limit = param.Limit > 0 ? param.Limit : 100;
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;

            var predicate = BuildPredicate(param);
            return await context!.CaseManagements!
                .AsExpandable()
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetCaseCount(VMCaseManagement param)
        {
            var predicate = BuildPredicate(param);
            return await context!.CaseManagements!
                .AsExpandable()
                .Where(predicate)
                .CountAsync();
        }

        public async Task<MCaseManagement> AddCase(MCaseManagement caseManagement)
        {
            caseManagement.OrgId = orgId;
            await context!.CaseManagements!.AddAsync(caseManagement);
            await context.SaveChangesAsync();
            return caseManagement;
        }

        public async Task<MCaseManagement?> UpdateCaseStatusById(string caseId, string status, string updatedBy)
        {
            Guid id = Guid.Parse(caseId);
            var existing = await context!.CaseManagements!
                .AsExpandable()
                .Where(p => p.Id!.Equals(id))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Status = status;
                existing.UpdatedBy = updatedBy;
                existing.UpdatedDate = DateTime.UtcNow;
                if (status == "Closed" || status == "Resolved")
                {
                    existing.ClosedBy = updatedBy;
                    existing.ClosedDate = DateTime.UtcNow;
                }
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MCaseManagementComment> AddComment(MCaseManagementComment comment)
        {
            await context!.CaseManagementComments!.AddAsync(comment);
            await context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<MCaseManagementComment>> GetComments(Guid caseId)
        {
            return await context!.CaseManagementComments!
                .AsExpandable()
                .Where(c => c.CaseId!.Equals(caseId))
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        private ExpressionStarter<MCaseManagement> BuildPredicate(VMCaseManagement param)
        {
            var pd = PredicateBuilder.New<MCaseManagement>(true);

            // org filter: apply only when orgId was explicitly set (not the reserve value)
            if (!orgId.Equals(RESERVE_ORG_ID))
            {
                pd = pd.And(p => p.OrgId!.Equals(orgId));
            }
            else if (!string.IsNullOrEmpty(param.OrgIdFilter))
            {
                pd = pd.And(p => p.OrgId!.Equals(param.OrgIdFilter));
            }

            if (!string.IsNullOrEmpty(param.Status))
                pd = pd.And(p => p.Status!.Equals(param.Status));

            if (!string.IsNullOrEmpty(param.Priority))
                pd = pd.And(p => p.Priority!.Equals(param.Priority));

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var fts = param.FullTextSearch;
                var ftsPd = PredicateBuilder.New<MCaseManagement>();
                ftsPd = ftsPd.Or(p => p.Ref!.Contains(fts));
                ftsPd = ftsPd.Or(p => p.Subject!.Contains(fts));
                ftsPd = ftsPd.Or(p => p.CreatedBy!.Contains(fts));
                pd = pd.And(ftsPd);
            }

            if (param.FromDate.HasValue)
                pd = pd.And(p => p.CreatedDate >= param.FromDate);

            if (param.ToDate.HasValue)
                pd = pd.And(p => p.CreatedDate <= param.ToDate);

            return pd;
        }
    }
}
