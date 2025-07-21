using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class PricingPlanRepository : BaseRepository, IPricingPlanRepository
    {
        public PricingPlanRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MPricingPlan AddPricingPlan(MPricingPlan pp)
        {
            pp.Id = Guid.NewGuid();
            pp.CreatedDate = DateTime.UtcNow;
            pp.UpdatedDate = DateTime.UtcNow;
            pp.OrgId = orgId;

            context!.PricingPlans!.Add(pp);
            context.SaveChanges();

            return pp;
        }

        private ExpressionStarter<MPricingPlan> PricingPlanPredicate(VMPricingPlan param)
        {
            var pd = PredicateBuilder.New<MPricingPlan>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPricingPlan>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.Status != null) && (param.Status > 0))
            {
                var statusPd = PredicateBuilder.New<MPricingPlan>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            return pd;
        }

        public int GetPricingPlanCount(VMPricingPlan param)
        {
            var predicate = PricingPlanPredicate(param);
            var cnt = context!.PricingPlans!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MPricingPlan> GetPricingPlans(VMPricingPlan param)
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

            var predicate = PricingPlanPredicate(param!);
            var arr = context!.PricingPlans!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MPricingPlan GetPricingPlanById(string ppId)
        {
            Guid id = Guid.Parse(ppId);

            var u = context!.PricingPlans!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MPricingPlan GetPricingPlanByName(string code)
        {
            var u = context!.PricingPlans!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsPricingPlanCodeExist(string code)
        {
            var cnt = context!.PricingPlans!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MPricingPlan? DeletePricingPlanById(string PricingPlanId)
        {
            Guid id = Guid.Parse(PricingPlanId);

            var r = context!.PricingPlans!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.PricingPlans!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MPricingPlan? UpdatePricingPlanById(string ppId, MPricingPlan pp)
        {
            Guid id = Guid.Parse(ppId);
            var result = context!.PricingPlans!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Description = pp.Description;
                result.Status = pp.Status;
                result.Priority = pp.Priority;
                result.Tags = pp.Tags;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}