using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IVoucherService
    {
        public Task<MVVoucher> AddVoucher(string orgId, MVoucher vc);
        public Task<List<MVoucher>> GetVouchers(string orgId, VMVoucher param);
        public Task<int> GetVoucherCount(string orgId, VMVoucher param);
        public Task<MVVoucher> DeleteVoucherById(string orgId, string voucherId);
        public Task<MVVoucher> UpdateVoucherStatusById(string orgId, string voucherId, string status);
        public Task<MVVoucher> GetVoucherById(string orgId, string voucherId);
        public Task<MVVoucher> VerifyVoucherByBarcode(string orgId, string barcode);
        public Task<MVVoucher> VerifyVoucherByPin(string orgId, string voucherNo, string pin);
        public Task<MVVoucher> UpdateVoucherUsedFlagById(string orgId, string voucherId, string isUsed);
        public Task<MVVoucher> UpdateVoucherUsedFlagById(string orgId, string voucherId, string pin, string isUsed);
        public Task<MVVoucher> GetVoucherVerifyUrl(string id, string voucherId, bool isQrCode);
    }
}
