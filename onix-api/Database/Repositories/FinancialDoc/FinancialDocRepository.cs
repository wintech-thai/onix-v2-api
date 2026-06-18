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
    }
}