using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ItemService : BaseService, IItemService
    {
        private readonly IItemRepository? repository = null;

        public ItemService(IItemRepository repo) : base()
        {
            repository = repo;
        }

        public MItem GetItemById(string orgId, string itemId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemById(itemId);

            return result;
        }

        public MVItem? AddItem(string orgId, MItem cycle)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVItem();

            var isExist = repository!.IsItemCodeExist(cycle.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Item code [{cycle.Code}] is duplicate";

                return r;
            }

            var result = repository!.AddItem(cycle);

            r.Status = "OK";
            r.Description = "Success";
            r.Item = result;

            return r;
        }

        public MVItem? UpdateItemById(string orgId, string itemId, MItem cycle)
        {
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateItemById(itemId, cycle);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Item = result;
            return r;
        }

        public MVItem? DeleteItemById(string orgId, string itemId)
        {
            var r = new MVItem()
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
            var m = repository!.DeleteItemById(itemId);

            r.Item = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MItem> GetItems(string orgId, VMItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItems(param);

            return result;
        }

        public int GetItemCount(string orgId, VMItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemCount(param);

            return result;
        }
    }
}
