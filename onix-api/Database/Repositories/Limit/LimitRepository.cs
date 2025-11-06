using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class LimitRepository : BaseRepository, ILimitRepository
    {
        public LimitRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MLimit> LimitPredicate(VMLimit param)
        {
            var pd = PredicateBuilder.New<MLimit>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.StatCode != "") && (param.StatCode != null))
            {
                var fullTextPd = PredicateBuilder.New<MLimit>();
                fullTextPd = fullTextPd.Or(p => p.StatCode!.Equals(param.StatCode));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<int> GetLimitCount(VMLimit param)
        {
            var predicate = LimitPredicate(param);
            var cnt = await context!.Limits!.Where(predicate).AsExpandable().CountAsync();

            return cnt;
        }

        public async Task<IEnumerable<MLimit>> GetLimits(VMLimit param)
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

            var predicate = LimitPredicate(param!);
            var arr = await context!.Limits!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .AsExpandable().ToListAsync();

            return arr;
        }

        public async Task<MLimit> UpsertLimit(MLimit limit)
        {
            limit.OrgId = orgId;

            var result = await context!.Limits!.Where(x => x.OrgId!.Equals(orgId) && x.StatCode!.Equals(limit.StatCode)).AsExpandable().FirstOrDefaultAsync();
            if (result == null)
            {
                //Add here
                await context!.Limits!.AddAsync(limit);
            }
            else
            {
                //Update here
                result.Limit = limit.Limit;
            }            

            await context.SaveChangesAsync();

            return limit;
        }
    }
}