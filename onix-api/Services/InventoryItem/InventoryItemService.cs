using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class InventoryItemService : BaseService, IInventoryItemService
    {
        private readonly IInventoryItemRepository? repository = null;

        public InventoryItemService(IInventoryItemRepository repo) : base()
        {
            repository = repo;
        }

        // แปลง raw base64 ให้เป็น data URI พร้อมใช้ใน <img src> แล้วเคลียร์ raw base64 ทิ้ง
        // ไม่ให้ response มีข้อมูลรูปซ้ำซ้อนกัน (pattern เดียวกับ PaymentDocumentService)
        private static void BuildPreviewUrl(MInventoryItem inventoryItem)
        {
            if (string.IsNullOrEmpty(inventoryItem.ImageBase64))
            {
                return;
            }

            var mimeType = string.IsNullOrEmpty(inventoryItem.MimeType) ? "image/jpeg" : inventoryItem.MimeType;
            inventoryItem.PreviewUrl = $"data:{mimeType};base64,{inventoryItem.ImageBase64}";
            inventoryItem.ImageBase64 = null;
        }

        public MInventoryItem GetInventoryItemById(string orgId, string inventoryItemId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetInventoryItemById(inventoryItemId);

            if (result != null)
            {
                BuildPreviewUrl(result);
            }

            return result!;
        }

        public MVInventoryItem? AddInventoryItem(string orgId, MInventoryItem inventoryItem)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVInventoryItem();

            if (repository!.IsInventoryItemCodeExist(inventoryItem.Code!))
            {
                r.Status = "DUPLICATE";
                r.Description = $"Inventory Item code [{inventoryItem.Code}] is duplicate";

                return r;
            }

            var result = repository!.AddInventoryItem(inventoryItem);
            BuildPreviewUrl(result);

            r.Status = "OK";
            r.Description = "Success";
            r.InventoryItem = result;

            return r;
        }

        public MVInventoryItem? UpdateInventoryItemById(string orgId, string inventoryItemId, MInventoryItem inventoryItem)
        {
            var r = new MVInventoryItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateInventoryItemById(inventoryItemId, inventoryItem);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Inventory Item ID [{inventoryItemId}] not found for the organization [{orgId}]";

                return r;
            }

            BuildPreviewUrl(result);
            r.InventoryItem = result;
            return r;
        }

        public MVInventoryItem? DeleteInventoryItemById(string orgId, string inventoryItemId)
        {
            var r = new MVInventoryItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(inventoryItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Inventory Item ID [{inventoryItemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteInventoryItemById(inventoryItemId);

            r.InventoryItem = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Inventory Item ID [{inventoryItemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MInventoryItem> GetInventoryItems(string orgId, VMInventoryItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetInventoryItems(param);

            foreach (var item in result)
            {
                BuildPreviewUrl(item);
            }

            return result;
        }

        public int GetInventoryItemCount(string orgId, VMInventoryItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetInventoryItemCount(param);

            return result;
        }
    }
}
