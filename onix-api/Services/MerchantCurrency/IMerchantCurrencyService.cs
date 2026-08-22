using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IMerchantCurrencyService
    {
        public List<MCurrency> GetAvailableCurrencies(string category);
        public Task<MVMerchantCurrency?> GetCurrencyById(string orgId, string merchantCurrencyId);
        public Task<MVMerchantCurrency> AddCurrency(string orgId, MMerchantCurrency currency);
        public Task<List<MMerchantCurrency>> GetCurrenciesByMerchantId(string orgId, string merchantId);
        public Task<MVMerchantCurrency> UpdateCurrencyById(string orgId, string merchantCurrencyId, MMerchantCurrency currency);
        public Task<MVMerchantCurrency?> UpdateCurrencyStatusById(string orgId, string merchantCurrencyId, string status);
    }
}
