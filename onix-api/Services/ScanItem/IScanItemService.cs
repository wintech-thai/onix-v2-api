using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IScanItemService
    {
        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin);
        public MVScanItem AttachScanItemToProduct(string orgId, string itemId, string productId);
        public MVScanItem AttachScanItemToCustomer(string orgId, string itemId, string customerId);
        public MVItem GetScanItemProduct(string orgId, string serial, string pin, string otp);
        public MVEntity GetScanItemCustomer(string orgId, string serial, string pin, string otp);
        public MVOtp GetOtpViaEmail(string orgId, string serial, string pin, string otp, string email);
        public MVEntity RegisterCustomer(string id, string serial, string pin, string otp, MCustomerRegister cust);

        //ถ้าจะมี GetScanItems() ตอนที่ดึงค่า PIN มาแสดงให้ทำการ masking PIN เสมอ !!!!!
    }
}
