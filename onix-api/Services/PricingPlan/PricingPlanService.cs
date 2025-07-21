using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class PricingPlanService : BaseService, IPricingPlanService
    {
        private readonly IPricingPlanRepository? repository = null;

        public PricingPlanService(IPricingPlanRepository repo) : base()
        {
            repository = repo;
        }

        public MPricingPlan GetPricingPlanById(string orgId, string ppId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetPricingPlanById(ppId);

            return result;
        }

        public MVPricingPlan? AddPricingPlan(string orgId, MPricingPlan pp)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPricingPlan();

            var isExist = repository!.IsPricingPlanCodeExist(pp.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"PricingPlan code [{pp.Code}] is duplicate";

                return r;
            }

            var result = repository!.AddPricingPlan(pp);

            r.Status = "OK";
            r.Description = "Success";
            r.PricingPlan = result;

            return r;
        }

        public MVPricingPlan? UpdatePricingPlanById(string orgId, string ppId, MPricingPlan pp)
        {
            var r = new MVPricingPlan()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdatePricingPlanById(ppId, pp);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"PricingPlan ID [{ppId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PricingPlan = result;
            return r;
        }

        public MVPricingPlan? DeletePricingPlanById(string orgId, string ppId)
        {
            var r = new MVPricingPlan()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(ppId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"PricingPlan ID [{ppId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeletePricingPlanById(ppId);

            r.PricingPlan = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"PricingPlan ID [{ppId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MPricingPlan> GetPricingPlans(string orgId, VMPricingPlan param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetPricingPlans(param);

            return result;
        }

        public int GetPricingPlanCount(string orgId, VMPricingPlan param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetPricingPlanCount(param);

            return result;
        }
    }
}
