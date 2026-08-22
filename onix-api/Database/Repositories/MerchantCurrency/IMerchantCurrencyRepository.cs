using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IMerchantCurrencyRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsCurrencyExist(string merchantId, string currencyCode);
        public Task<List<MMerchantCurrency>> GetCurrenciesByMerchantId(string merchantId);
        public Task<MMerchantCurrency> AddCurrency(MMerchantCurrency currency);
        public Task<MMerchantCurrency?> GetCurrencyById(string merchantCurrencyId);
        public Task<MMerchantCurrency?> UpdateCurrencyById(string merchantCurrencyId, MMerchantCurrency currency);
        public Task<MMerchantCurrency?> UpdateCurrencyStatusById(string merchantCurrencyId, string status);
    }
}
