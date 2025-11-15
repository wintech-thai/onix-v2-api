using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class StatRepository : BaseRepository, IStatRepository
    {
        public StatRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MStat> StatPredicate(VMStat param)
        {
            var pd = PredicateBuilder.New<MStat>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.StatCode != "") && (param.StatCode != null))
            {
                var fullTextPd = PredicateBuilder.New<MStat>();
                fullTextPd = fullTextPd.Or(p => p.StatCode!.Contains(param.StatCode));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public int GetStatCount(VMStat param)
        {
            var predicate = StatPredicate(param);
            var cnt = context!.Stats!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MStat> GetStats(VMStat param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = StatPredicate(param!);
            var arr = context!.Stats!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }
    }
}