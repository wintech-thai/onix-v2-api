using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IScanItemService
    {
        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin);
        public MVScanItem AttachScanItemToProduct(string orgId, string itemId, string productId);
    }
}
