using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface ICurrencyAccountService
    {
        public Task<MVCurrencyAccount> GetCurrencyAccountById(string orgId, string currencyAccountId);
        public List<MCryptoCurrency> GetAvailableCryptoCurrencies();
        public Task<MVCurrencyAccount> AddFiatCurrencyAccount(string orgId, MCurrencyAccount currencyAccount);
        public Task<MVCurrencyAccount> AddCryptoCurrencyAccount(string orgId, MCurrencyAccount currencyAccount);
        public Task<List<MCurrencyAccount>> GetCurrencyAccounts(string orgId, VMCurrencyAccount param);
        public Task<int> GetCurrencyAccountCount(string orgId, VMCurrencyAccount param);
        public Task<MVCurrencyAccount> UpdateCurrencyAccountById(string orgId, string currencyAccountId, MCurrencyAccount currencyAccount);
        public Task<MVCurrencyAccount?> UpdateCurrencyAccountStatusById(string orgId, string currencyAccountId, string status);
        public Task<MVCurrencyAccount> DeleteCurrencyAccountById(string orgId, string currencyAccountId);
    }
}
