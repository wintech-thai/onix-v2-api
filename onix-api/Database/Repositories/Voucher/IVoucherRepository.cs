using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IVoucherRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsVoucherNoExist(string docNo);
        public Task<MVoucher> AddVoucher(MVoucher vc);
        public Task<MVoucher?> UpdateVoucherStatusById(string voucherId, string status);
        public Task<MVoucher?> UpdateVoucherIsUsedFlagById(string voucherId, string isUseFlag);
        public Task<MVoucher?> DeleteVoucherById(string voucherId);
        public Task<List<MVoucher>> GetVouchers(VMVoucher param);
        public Task<int> GetVoucherCount(VMVoucher param);
        public Task<MVoucher?> GetVoucherById(string voucherId);
    }
}
