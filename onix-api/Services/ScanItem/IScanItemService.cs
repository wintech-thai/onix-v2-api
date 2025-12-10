using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemService
    {
        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin, bool isDryRun);
        public MVScanItem AttachScanItemToProduct(string orgId, string scanItemId, string productId);
        public MVScanItem AttachScanItemToCustomer(string orgId, string scanItemId, string customerId);
        public MVScanItem DetachScanItemFromCustomer(string orgId, string scanItemId);
        public MVScanItem DetachScanItemFromProduct(string orgId, string scanItemId);
        public MVItem GetScanItemProduct(string orgId, string serial, string pin, string otp);
        public MVEntity GetScanItemCustomer(string orgId, string serial, string pin, string otp);
        public MVOtp GetOtpViaEmail(string orgId, string serial, string pin, string otp, string email);
        public MVEntity RegisterCustomer(string id, string serial, string pin, string otp, MCustomerRegister cust);

        //ถ้าจะมี GetScanItems() ตอนที่ดึงค่า PIN มาแสดงให้ทำการ masking PIN เสมอ !!!!!

        public MVScanItem? GetScanItemById(string orgId, string scanItemId);
        public MVScanItem AddScanItem(string orgId, MScanItem scanItem);
        public MVScanItem DeleteScanItemById(string orgId, string scanItemId);
        public MVScanItem UnVerifyScanItemById(string orgId, string scanItemId);
        public int GetScanItemCount(string orgId, VMScanItem param);
        public IEnumerable<MScanItem> GetScanItems(string orgId, VMScanItem param);

        public Task<int> GetScanItemCountAsync(string orgId, VMScanItem param);
        public Task<IEnumerable<MScanItem>> GetScanItemsAsnyc(string orgId, VMScanItem param);

        public MVScanItem? GetScanItemUrlDryRunById(string orgId, string scanItemId);

        //=== V2 ===

        public Task<int> GetScanItemCountV2(string orgId, VMScanItem param);
        public Task<IEnumerable<MScanItem>> GetScanItemsV2(string orgId, VMScanItem param);
        public Task<MVScanItem> GetScanItemByIdV2(string orgId, string scanItemId);
        public Task<MVScanItem> AddScanItemV2(string orgId, MScanItem scanItem);
    }
}
