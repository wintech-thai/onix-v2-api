using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class PricingPlanItemService : BaseService, IPricingPlanItemService
    {
        private readonly IPricingPlanItemRepository? repository = null;

        public PricingPlanItemService(IPricingPlanItemRepository repo) : base()
        {
            repository = repo;
        }

        public MPricingPlanItem GetPricingPlanItemById(string orgId, string pricingPlanItemId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetPricingPlanItemById(pricingPlanItemId);

            return result;
        }

        public MVPricingPlanItem? AddPricingPlanItem(string orgId, MPricingPlanItem pricingPlanItem)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPricingPlanItem();
            var result = repository!.AddPricingPlanItem(pricingPlanItem);

            r.Status = "OK";
            r.Description = "Success";
            r.PricingPlanItem = result;

            return r;
        }

        public MVPricingPlanItem? UpdatePricingPlanItemById(string orgId, string pricingPlanItemId, MPricingPlanItem pricingPlanItem)
        {
            var r = new MVPricingPlanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdatePricingPlanItemById(pricingPlanItemId, pricingPlanItem);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item image ID [{pricingPlanItemId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PricingPlanItem = result;
            return r;
        }

        public MVPricingPlanItem? DeletePricingPlanItemByItemId(string orgId, string itemId)
        {
            var r = new MVPricingPlanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(itemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Item ID [{itemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeletePricingPlanItemByItemId(itemId);

            r.PricingPlanItems = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public MVPricingPlanItem? DeletePricingPlanItemById(string orgId, string pricingPlanItemId)
        {
            var r = new MVPricingPlanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(pricingPlanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Item image ID [{pricingPlanItemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeletePricingPlanItemById(pricingPlanItemId);

            r.PricingPlanItem = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item image ID [{pricingPlanItemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MPricingPlanItem> GetPricingPlanItems(string orgId, VMPricingPlanItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetPricingPlanItems(param);

            return result;
        }

        public int GetPricingPlanItemCount(string orgId, VMPricingPlanItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetPricingPlanItemCount(param);

            return result;
        }
    }
}
