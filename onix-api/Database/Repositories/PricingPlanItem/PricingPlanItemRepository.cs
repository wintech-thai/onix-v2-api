using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class PricingPlanItemRepository : BaseRepository, IPricingPlanItemRepository
    {
        public PricingPlanItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MPricingPlanItem AddPricingPlanItem(MPricingPlanItem item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            context!.PricingPlanItems!.Add(item);
            context.SaveChanges();

            return item;
        }

        private ExpressionStarter<MPricingPlanItem> PricingPlanItemPredicate(VMPricingPlanItem param)
        {
            Guid id = Guid.Parse(param.PricingPlanId!);
            var pd = PredicateBuilder.New<MPricingPlanItem>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            //Need to use PricePlanId as the query parameter
            var itemPd = PredicateBuilder.New<MPricingPlanItem>();
            itemPd = itemPd.Or(p => p.PricingPlanId!.Equals(id));
            pd = pd.And(itemPd);

            return pd;
        }

        public int GetPricingPlanItemCount(VMPricingPlanItem param)
        {
            var predicate = PricingPlanItemPredicate(param);
            var cnt = context!.PricingPlanItems!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MPricingPlanItem> GetPricingPlanItems(VMPricingPlanItem param)
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

            var predicate = PricingPlanItemPredicate(param!);
            var arr = context!.PricingPlanItems!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MPricingPlanItem GetPricingPlanItemById(string pricingPlanItemId)
        {
            Guid id = Guid.Parse(pricingPlanItemId);

            var u = context!.PricingPlanItems!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MPricingPlanItem? DeletePricingPlanItemById(string PricingPlanItemId)
        {
            Guid id = Guid.Parse(PricingPlanItemId);

            var r = context!.PricingPlanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.PricingPlanItems!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public List<MPricingPlanItem>? DeletePricingPlanItemByItemId(string pricingPlanItemId)
        {
            Guid id = Guid.Parse(pricingPlanItemId);

            var r = context!.PricingPlanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).ToList();
            if (r != null)
            {
                context!.PricingPlanItems!.RemoveRange(r);
                context.SaveChanges();
            }

            return r;
        }

        public MPricingPlanItem? UpdatePricingPlanItemById(string pricingPlanItemId, MPricingPlanItem item)
        {
            Guid id = Guid.Parse(pricingPlanItemId);
            var result = context!.PricingPlanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RateDefinition = item.RateDefinition;
                result.ItemId = item.ItemId;
                result.RateType = item.RateType;
                result.FlateRate = item.FlateRate;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}