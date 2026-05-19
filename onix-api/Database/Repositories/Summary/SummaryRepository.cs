using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class SummaryRepository : BaseRepository, ISummaryRepository
    {
        public SummaryRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<T> IsOrgMatchPredicate<T>() where T : IOrgEntity
        {
            var pd = PredicateBuilder.New<T>(true);
            if (orgId != "global")
            {
                //ต้องเอา orgId มา where ด้วย
                var orgPd = PredicateBuilder.New<T>(true);
                orgPd = orgPd.And(p => p.OrgId!.Equals(orgId));
                pd = pd.And(orgPd);
            }

            return pd;
        }

        public async Task<List<MAggregateData>> GetMerchantCountByStatus(VMSummary param)
        {
            var result = await context!.Merchants!
                .Where(IsOrgMatchPredicate<MMerchant>())
                .GroupBy(x => x.Status)
                .Select(g => new MAggregateData()
                {
                    AggregateKey1 = g.Key,
                    AggregateCount1 = g.Count()
                }).ToListAsync();

            return result;
        }

        public async Task<List<MAggregateData>> GetMerchantCount(VMSummary param)
        {
            var result = await context!.Merchants!
                .Where(IsOrgMatchPredicate<MMerchant>())
                .GroupBy(x => 1)
                .Select(g => new MAggregateData()
                {
                    AggregateCount1 = g.Count()
                }).ToListAsync();

            return result;
        }
    }
}