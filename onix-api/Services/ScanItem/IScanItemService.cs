using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemService
    {
        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin, bool isDryRun);
        public MVItem GetScanItemProduct(string orgId, string serial, string pin, string otp);
        public MVEntity GetScanItemCustomer(string orgId, string serial, string pin, string otp);
        public MVOtp GetOtpViaEmail(string orgId, string serial, string pin, string otp, string email);
        public MVEntity RegisterCustomer(string id, string serial, string pin, string otp, MCustomerRegister cust);

        //=== V2 ===
        public Task<MVScanItem> AttachScanItemToProduct(string orgId, string scanItemId, string productId);
        public Task<MVScanItem> AttachScanItemToCustomer(string orgId, string scanItemId, string customerId);
        public Task<MVScanItem> DetachScanItemFromCustomer(string orgId, string scanItemId);
        public Task<MVScanItem> DetachScanItemFromProduct(string orgId, string scanItemId);

        public Task<MVScanItem> GetScanItemUrlDryRunById(string orgId, string scanItemId);
        public Task<int> GetScanItemCountV2(string orgId, VMScanItem param);
        public Task<IEnumerable<MScanItem>> GetScanItemsV2(string orgId, VMScanItem param);
        public Task<MVScanItem> GetScanItemByIdV2(string orgId, string scanItemId);
        public Task<MVScanItem> AddScanItemV2(string orgId, MScanItem scanItem);
        public Task<MVScanItem> DeleteScanItemByIdV2(string orgId, string scanItemId);
        public Task<MVScanItem> UnVerifyScanItemByIdV2(string orgId, string scanItemId);
        public Task<MVScanItem> MoveScanItemToFolder(string orgId, string scanItemId, string folderId);
    }
}
