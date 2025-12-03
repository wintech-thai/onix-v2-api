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
    }
}
