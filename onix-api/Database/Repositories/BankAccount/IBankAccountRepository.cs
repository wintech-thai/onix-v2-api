using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IBankAccountRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsBankAccountNoExist(string accountNo);
        public Task<bool> IsBankAccountNameExist(string bankCode, string accountName);
        public Task<MBankAccount?> GetBankAccountByAccountNo(string accountNo);
        public Task<MBankAccount?> GetBankAccountByAccountName(string bankCode, string accountName);

        public Task<List<MBankAccount>> GetBankAccounts(VMBankAccount param);
        public Task<List<MBankAccount>> GetAllBankAccounts(VMBankAccount param);
        public Task<List<MBankAccountMerchant>> GetBankAccountSelectedMerchants(string bankAccountId);
        public Task<List<MBankAccountMerchant>> GetBankAccountsForMerchant(string merchantId);
        
        public Task<MBankAccountMerchant?> SelectMerchant(string bankAccountId, string merchantId);
        public Task<MBankAccountMerchant?> UnSelectMerchant(string bankAccountId, string merchantId);

        public Task<int> GetBankAccountCount(VMBankAccount param);
        public Task<MBankAccount?> GetBankAccountById(string bankAccountId);
        public Task<MBankAccount> AddBankAccount(MBankAccount bankAccount);
        public Task<MBankAccount?> DeleteBankAccountById(string bankAccountId);
        public Task<MBankAccount?> UpdateBankAccountById(string bankAccountId, MBankAccount bankAccount);
        public Task<MBankAccount?> UpdateBankAccountStatusById(string bankAccountId, string status);
    }
}
