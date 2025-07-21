using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPricingPlanRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MPricingPlan AddPricingPlan(MPricingPlan pp);
        public int GetPricingPlanCount(VMPricingPlan param);
        public IEnumerable<MPricingPlan> GetPricingPlans(VMPricingPlan param);
        public MPricingPlan GetPricingPlanById(string ppId);
        public MPricingPlan? DeletePricingPlanById(string ppId);
        public bool IsPricingPlanCodeExist(string ppName);
        public MPricingPlan? UpdatePricingPlanById(string ppId, MPricingPlan pp);
    }
}
