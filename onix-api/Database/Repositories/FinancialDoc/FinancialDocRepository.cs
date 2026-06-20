using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class FinancialDocRepository : BaseRepository, IFinancialDocRepository
    {
        public FinancialDocRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsDocNoExist(string docNo)
        {
            var exists = await context!.FinancialDocs!.AsExpandable().AnyAsync(p => p!.DocumentNo!.Equals(docNo) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MFinancialDoc> AddFinancialDoc(MFinancialDoc financialDoc)
        {
            financialDoc.OrgId = orgId;
            financialDoc.CreatedDate = DateTime.UtcNow;

            await context!.FinancialDocs!.AddAsync(financialDoc);
            await context.SaveChangesAsync();

            return financialDoc;
        }

        public async Task<MFinancialDoc?> GetFinancialDocById(string financialDocId)
        {
            Guid id = Guid.Parse(financialDocId);
            var u = await context!.FinancialDocs!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MFinancialDoc?> UpdateFinancialDocById(string financialDocId, MFinancialDoc financialDoc)
        {
            Guid id = Guid.Parse(financialDocId);
            var existing = await context!.FinancialDocs!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                //Not allow to update DocumentNo
                existing.Description = financialDoc.Description;
                existing.Tags = financialDoc.Tags;
                existing.FromDate = financialDoc.FromDate;
                existing.ToDate = financialDoc.ToDate;
                existing.ExpenseItemsDefinition = financialDoc.ExpenseItemsDefinition;
                existing.RevenueItemsDefinition = financialDoc.RevenueItemsDefinition;
                existing.SharingItemsDefinition = financialDoc.SharingItemsDefinition;
                existing.TotalRevenue = financialDoc.TotalRevenue;
                existing.TotalExpense = financialDoc.TotalExpense;
                existing.ProfitLoss = financialDoc.ProfitLoss;

                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MFinancialDoc?> DeleteFinancialDocById(string financialDocId)
        {
            Guid id = Guid.Parse(financialDocId);
            var existing = await context!.FinancialDocs!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                context.FinancialDocs!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        private ExpressionStarter<MFinancialDoc> FinancialDocPredicate(VMFinancialDoc param)
        {
            var pd = PredicateBuilder.New<MFinancialDoc>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MFinancialDoc>();
                fullTextPd = fullTextPd.Or(p => p.DocumentNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<MFinancialDoc>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<MFinancialDoc>();
                toDatePd = toDatePd.Or(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            return pd;
        }

        public async Task<List<MFinancialDoc>> GetFinancialDocs(VMFinancialDoc param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = FinancialDocPredicate(param!);
            var result = await context!.FinancialDocs!.AsExpandable()
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return result;
        }

        public async Task<int> GetFinancialDocCount(VMFinancialDoc param)
        {
            var predicate = FinancialDocPredicate(param!);
            var result = await context!.FinancialDocs!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }
    }
}