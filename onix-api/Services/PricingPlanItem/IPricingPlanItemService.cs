using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPricingPlanItemService
    {
        public MPricingPlanItem GetPricingPlanItemById(string orgId, string pricingPlanItemId);
        public MVPricingPlanItem? AddPricingPlanItem(string orgId, MPricingPlanItem pricingPlanItem);
        public MVPricingPlanItem? DeletePricingPlanItemById(string orgId, string pricingPlanItemId);
        public MVPricingPlanItem? DeletePricingPlanItemByItemId(string orgId, string itemId);
        public IEnumerable<MPricingPlanItem> GetPricingPlanItems(string orgId, VMPricingPlanItem param);
        public int GetPricingPlanItemCount(string orgId, VMPricingPlanItem param);
        public MVPricingPlanItem? UpdatePricingPlanItemById(string orgId, string pricingPlanItemId, MPricingPlanItem pricingPlanItem);
    }
}
