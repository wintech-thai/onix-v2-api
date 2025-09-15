using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;

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

            result.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(result.Properties!);
            result.Properties = "";

            return result;
        }

        public MVItem? AddItem(string orgId, MItem item)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVItem();

            var isExist = repository!.IsItemCodeExist(item.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Item code [{item.Code}] is duplicate";

                return r;
            }

            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);

            var result = repository!.AddItem(item);
            result.Properties = "";

            r.Status = "OK";
            r.Description = "Success";
            r.Item = result;

            return r;
        }

        public MVItem? UpdateItemById(string orgId, string itemId, MItem item)
        {
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);
            var result = repository!.UpdateItemById(itemId, item);
            
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";

                return r;
            }

            result.Properties = "";
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

            foreach (var item in result)
            {
                item.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(item.Properties!);
                item.Properties = "";
            }

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
