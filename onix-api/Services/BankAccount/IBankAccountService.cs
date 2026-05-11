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
        public Task<int> GetBankAccountCount(string orgId, VMBankAccount param);
        public Task<MVBankAccount> UpdateBankAccountById(string orgId, string bankAccountId, MBankAccount bankAccount);
        public Task<MVBankAccount?> UpdateBankAccountStatusById(string bankAccountId, string status);
    }
}
