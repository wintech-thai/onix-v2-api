using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<MScanItem?> AttachScanItemToProduct(string itemId, string productId, MItem product);
        public Task<MScanItem?> AttachScanItemToCustomer(string itemId, string customerId, MEntity customer);
        public Task<MScanItem?> DetachScanItemFromCustomer(string itemId);
        public Task<MScanItem?> DetachScanItemFromProduct(string itemId);

        public Task<MScanItem?> GetScanItemBySerialPinV2(string serial, string pin);
        public Task<MScanItem?> RegisterScanItemV2(string itemId);
        public Task<MScanItem?> IncreaseScanCountV2(string itemId);

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
