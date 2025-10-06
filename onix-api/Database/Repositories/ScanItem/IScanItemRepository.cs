using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public MScanItem RegisterScanItem(string itemId);
        public MScanItem IncreaseScanCount(string itemId);
        public MScanItem AttachScanItemToProduct(string itemId, string productId, MItem product);
        public MScanItem AttachScanItemToCustomer(string itemId, string customerId, MEntity customer);
        public MScanItem? GetScanItemBySerialPin(string serial, string pin);

        public MScanItem AddScanItem(MScanItem scanItem);
        public int GetScanItemCount(VMScanItem param);
        public IEnumerable<MScanItem> GetScanItems(VMScanItem param);
        public MScanItem GetScanItemById(string scanItemId);
        public MScanItem? DeleteScanItemById(string scanItemId);
        public MScanItem? UnVerifyScanItemById(string scanItemId);
        public bool IsSerialExist(string serial);
        public bool IsPinExist(string pin);
    }
}
