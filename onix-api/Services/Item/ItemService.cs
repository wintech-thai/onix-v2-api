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

            if (!string.IsNullOrEmpty(result.Properties))
            {
                result.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(result.Properties!);
            }

            if (!string.IsNullOrEmpty(result.Narrative))
            {
                result.Narratives = [.. result.Narrative.Split('|')];
            }

            result.Properties = "";
            result.Narrative = "";

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

            if (item.PropertiesObj == null)
            {
                item.PropertiesObj = new MItemProperties();
            }
            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);
            item.Narrative = string.Join("|", item.Narratives ?? Array.Empty<string>());

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

            if (item.PropertiesObj == null)
            {
                item.PropertiesObj = new MItemProperties();
            }

            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);
            item.Narrative = string.Join("|", item.Narratives ?? Array.Empty<string>());

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
                // ไม่จำเป็นต้องส่งออกไป
                //if (!string.IsNullOrEmpty(item.Properties))
                //{
                //    item.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(item.Properties!);
                //}

                item.Properties = "";
                
                //เพื่อไม่ให้ข้อมูลที่ response กลับไปใหญ่จนเกินไป
                item.Narrative = "";
                item.Content = "";
            }

            return result;
        }

        public int GetItemCount(string orgId, VMItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemCount(param);

            return result;
        }

        public IEnumerable<NameValue> GetAllowItemPropertyNames(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var props = new NameValue[]
            {
                new() { Name = "DimensionUnit", Value = "cm" },
                new() { Name = "WeightUnit", Value = "gram" },
                new() { Name = "Category", Value = "" },
                new() { Name = "SupplierUrl", Value = "" },
                new() { Name = "ProductUrl", Value = "" },
                new() { Name = "Width", Value = "" },
                new() { Name = "Weight", Value = "" },
            };

            return props;
        }
    }
}
