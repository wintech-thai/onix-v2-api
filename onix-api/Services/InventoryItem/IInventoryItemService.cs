using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IInventoryItemService
    {
        public MInventoryItem GetInventoryItemById(string orgId, string inventoryItemId);
        public MVInventoryItem? AddInventoryItem(string orgId, MInventoryItem inventoryItem);
        public MVInventoryItem? DeleteInventoryItemById(string orgId, string inventoryItemId);
        public IEnumerable<MInventoryItem> GetInventoryItems(string orgId, VMInventoryItem param);
        public int GetInventoryItemCount(string orgId, VMInventoryItem param);
        public MVInventoryItem? UpdateInventoryItemById(string orgId, string inventoryItemId, MInventoryItem inventoryItem);
    }
}
