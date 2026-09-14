using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IInventoryItemRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MInventoryItem AddInventoryItem(MInventoryItem inventoryItem);
        public int GetInventoryItemCount(VMInventoryItem param);
        public IEnumerable<MInventoryItem> GetInventoryItems(VMInventoryItem param);
        public MInventoryItem GetInventoryItemById(string inventoryItemId);
        public MInventoryItem? DeleteInventoryItemById(string inventoryItemId);
        public bool IsInventoryItemCodeExist(string code);
        public MInventoryItem? UpdateInventoryItemById(string inventoryItemId, MInventoryItem inventoryItem);
    }
}
