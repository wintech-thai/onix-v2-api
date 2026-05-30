using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IBankAccountService
    {
        public Task<MVBankAccount> GetBankAccountById(string orgId, string bankAccountId);
        public Task<MVBankAccount> AddBankAccount(string orgId, MBankAccount bankAccount);
        public Task<MVBankAccount> DeleteBankAccountById(string orgId, string bankAccountId);
        public Task<List<MBankAccount>> GetBankAccounts(string orgId, VMBankAccount param);
        public List<MBank> GetAvailableBanks();
        public List<MBank> GetAvailableSupportQrBanks();
        public Task<int> GetBankAccountCount(string orgId, VMBankAccount param);
        public Task<MVBankAccount> UpdateBankAccountById(string orgId, string bankAccountId, MBankAccount bankAccount);
        public Task<MVBankAccount?> UpdateBankAccountStatusById(string orgId, string bankAccountId, string status);
        public Task<List<MBankAccountMerchant>> GetMerchantsForBankAccount(string orgId, string bankAccountId);
        public Task<List<MBankAccountMerchant>> GetPayInBankAccountsForMerchant(string orgId, string merchantId);
        public Task<List<MBankAccountMerchant>> GetPayInBankAccountsWithGlobalForMerchant(string orgId, string merchantId);
        public Task<List<MBankAccount>> GetPayInBankAccountsWithGlobalAll(string orgId);
        public Task<List<MBankAccountMerchant>> GetPayOutBankAccountsForMerchant(string orgId, string merchantId);
        public Task<MVBankAccountMerchant?> SelectMerchant(string orgId, string bankAccountId, string merchantId);
        public Task<MVBankAccountMerchant?> UnSelectMerchant(string orgId,string bankAccountId, string merchantId);
    }
}
