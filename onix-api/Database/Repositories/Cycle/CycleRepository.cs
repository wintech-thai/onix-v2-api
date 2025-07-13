using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class CycleRepository : BaseRepository, ICycleRepository
    {
        public CycleRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MCycle AddCycle(MCycle clcye)
        {
            clcye.Id = Guid.NewGuid();
            clcye.CreatedDate = DateTime.UtcNow;
            clcye.UpdatedDate = DateTime.UtcNow;
            clcye.OrgId = orgId;

            context!.Cycles!.Add(clcye);
            context.SaveChanges();

            return clcye;
        }

        private ExpressionStarter<MCycle> CyclePredicate(VMCycle param)
        {
            var pd = PredicateBuilder.New<MCycle>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MCycle>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.CycleType != null) && (param.CycleType > 0))
            {
                var refTypePd = PredicateBuilder.New<MCycle>();
                refTypePd = refTypePd.Or(p => p.CycleType!.Equals(param.CycleType));

                pd = pd.And(refTypePd);
            }

            return pd;
        }

        public int GetCycleCount(VMCycle param)
        {
            var predicate = CyclePredicate(param);
            var cnt = context!.Cycles!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MCycle> GetCycles(VMCycle param)
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

            var predicate = CyclePredicate(param!);
            var arr = context!.Cycles!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MCycle GetCycleById(string clcyeId)
        {
            Guid id = Guid.Parse(clcyeId);

            var u = context!.Cycles!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MCycle GetCycleByName(string code)
        {
            var u = context!.Cycles!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsCycleCodeExist(string code)
        {
            var cnt = context!.Cycles!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MCycle? DeleteCycleById(string CycleId)
        {
            Guid id = Guid.Parse(CycleId);

            var r = context!.Cycles!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.Cycles!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MCycle? UpdateCycleById(string clcyeId, MCycle clcye)
        {
            Guid id = Guid.Parse(clcyeId);
            var result = context!.Cycles!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Description = clcye.Description;
                result.StargDate = clcye.StargDate;
                result.EndDate = clcye.EndDate;
                result.Tags = clcye.Tags;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}