using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ICurrencyAcountRepository
    {
        public void SetCustomOrgId(string customOrgId);

        //==== Fiat ====
        public Task<bool> IsFiatCurrencyAccountNoExist(string currencyCode, string bankCode, string accountNo);
        public Task<bool> IsFiatCurrencyAccountNameExist(string currencyCode, string bankCode, string accountName);
        public Task<MCurrencyAccount?> GetFiatCurrencyAccountByAccountNo(string currencyCode, string bankCode, string accountNo);
        public Task<MCurrencyAccount?> GetFiatCurrencyAccountByAccountName(string currencyCode, string bankCode, string accountName);
        public Task<MCurrencyAccount?> UpdateCurrencyAccountBankConfigById(string currencyAccountId, string bankConfig);


        //==== Common ====
        public Task<List<MCurrencyAccount>> GetAllCurrencyAccounts(VMCurrencyAccount param);
        public Task<List<MCurrencyAccount>> GetCurrencyAccounts(VMCurrencyAccount param);
        public Task<int> GetCurrencyAccountCount(VMCurrencyAccount param);
        public Task<MCurrencyAccount?> DeleteCurrencyAccountById(string currencyAccountId);
        public Task<MCurrencyAccount?> GetCurrencyAccountById(string currencyAccountId);
        public Task<MCurrencyAccount> AddCurrencyAccount(MCurrencyAccount currencyAccount);
        public Task<MCurrencyAccount?> UpdateCurrencyAccountById(string currencyAccountId, MCurrencyAccount currencyAccount);
        public Task<MCurrencyAccount?> UpdateCurrencyAccountStatusById(string currencyAccountId, string status);


        //==== Crypto ====
        public Task<bool> IsCrypotCurrencyEpkExist(string currencyCode, string epk);
        public Task<MCurrencyAccount?> GetCryptoCurrencyAccountByEpk(string currencyCode, string epk);

/*
        public Task<List<MBankAccountMerchant>> GetMerchantsForBankAccount(string bankAccountId);
        public Task<List<MBankAccountMerchant>> GetPayInBankAccountsForMerchant(string merchantId);
        public Task<List<MBankAccountMerchant>> GetPayInBankAccountsAll();
        public Task<List<MBankAccountMerchant>> GetPayOutBankAccountsForMerchant(string merchantId);

        public Task<List<MBankAccountMerchant>> GetMerchantCountByBankAccountId();
        public Task<List<MBankAccountMerchant>> GetBankAccountCountByMerchantId();

        public Task<MBankAccountMerchant?> SelectMerchant(string bankAccountId, string merchantId);
        public Task<MBankAccountMerchant?> UnSelectMerchant(string bankAccountId, string merchantId);
*/
    }
}
