using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPricingPlanService
    {
        public MPricingPlan GetPricingPlanById(string orgId, string ppId);
        public MVPricingPlan? AddPricingPlan(string orgId, MPricingPlan pp);
        public MVPricingPlan? DeletePricingPlanById(string orgId, string ppId);
        public IEnumerable<MPricingPlan> GetPricingPlans(string orgId, VMPricingPlan param);
        public int GetPricingPlanCount(string orgId, VMPricingPlan param);
        public MVPricingPlan? UpdatePricingPlanById(string orgId, string ppId, MPricingPlan item);
    }
}
