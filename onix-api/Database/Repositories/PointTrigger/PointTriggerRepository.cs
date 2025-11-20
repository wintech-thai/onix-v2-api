using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PointTriggerRepository : BaseRepository, IPointTriggerRepository
    {
        public PointTriggerRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MPointTrigger> AddPointTrigger(MPointTrigger pt)
        {
            pt.OrgId = orgId;
            pt.TriggerDate = DateTime.UtcNow;

            await context!.PointTriggers!.AddAsync(pt);
            await context.SaveChangesAsync();

            return pt;
        }

        private ExpressionStarter<MPointTrigger> PointTriggerPredicate(VMPointTrigger param)
        {
            var pd = PredicateBuilder.New<MPointTrigger>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPointTrigger>();
                fullTextPd = fullTextPd.Or(p => p.TriggerName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<List<MPointTrigger>> GetPointTriggers(VMPointTrigger param)
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

            var predicate = PointTriggerPredicate(param!);
            var result = await context!.PointTriggers!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            //มันใหญ่มากเลยไม่อยากให้ return TriggerParams ออกไป
            foreach (var r in result)
            {
                r.TriggerParams = "";
            }

            return result;
        }

        public async Task<int> GetPointTriggerCount(VMPointTrigger param)
        {
            var predicate = PointTriggerPredicate(param!);
            var result = await context!.PointTriggers!.Where(predicate).CountAsync();

            return result;
        }

        public async Task<MPointTrigger?> GetPointTriggerById(string triggerId)
        {
            Guid id = Guid.Parse(triggerId);
            var u = await context!.PointTriggers!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<bool> IsTriggerNameExist(string triggerName)
        {
            var result = await context!.PointTriggers!.Where(x => x.OrgId!.Equals(orgId) && x.TriggerName!.Equals(triggerName)).FirstOrDefaultAsync();
            return result != null;
        }
    }
}
