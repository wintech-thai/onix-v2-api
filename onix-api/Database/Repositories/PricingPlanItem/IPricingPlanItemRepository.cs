using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPricingPlanItemRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MPricingPlanItem AddPricingPlanItem(MPricingPlanItem pricingPlanId);
        public int GetPricingPlanItemCount(VMPricingPlanItem param);
        public IEnumerable<MPricingPlanItem> GetPricingPlanItems(VMPricingPlanItem param);
        public MPricingPlanItem GetPricingPlanItemById(string pricingPlanId);
        public MPricingPlanItem? DeletePricingPlanItemById(string pricingPlanId);
        public List<MPricingPlanItem>? DeletePricingPlanItemByItemId(string itemId);
        public MPricingPlanItem? UpdatePricingPlanItemById(string pricingPlanId, MPricingPlanItem pricingPlan);
    }
}
