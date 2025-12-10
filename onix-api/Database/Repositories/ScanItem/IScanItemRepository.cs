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
        public MScanItem DetachScanItemFromCustomer(string itemId);
        public MScanItem DetachScanItemFromProduct(string itemId);
        public MScanItem? GetScanItemBySerialPin(string serial, string pin);


        public Task<IEnumerable<MScanItem>> GetScanItemsV2(VMScanItem param);
        public Task<int> GetScanItemCountV2(VMScanItem param);
        public Task<MScanItem> GetScanItemByIdV2(string scanItemId);
        public Task<MScanItem> AddScanItemV2(MScanItem scanItem);
        public Task<bool> IsSerialExistV2(string serial);
        public Task<bool> IsPinExistV2(string pin);
        public Task<MScanItem?> DeleteScanItemByIdV2(string scanItemId);
        public Task<MScanItem?> UnVerifyScanItemByIdV2(string scanItemId);
    }
}
