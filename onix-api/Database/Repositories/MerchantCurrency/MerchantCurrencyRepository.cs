using LinqKit;
using Its.Onix.Api.Models;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class MerchantCurrencyRepository : BaseRepository, IMerchantCurrencyRepository
    {
        public MerchantCurrencyRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsCurrencyExist(string merchantId, string currencyCode)
        {
            var exists = await context!.MerchantCurrencies!
                .AsExpandable()
                .AnyAsync(p => p!.Currency!.Equals(currencyCode) && p!.MerchantId!.Equals(merchantId) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<List<MMerchantCurrency>> GetCurrenciesByMerchantId(string merchantId)
        {
            var result = await context!.MerchantCurrencies!
                .AsExpandable()
                .Where(p => p.MerchantId!.Equals(merchantId))
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<MMerchantCurrency> AddCurrency(MMerchantCurrency currency)
        {
            currency.OrgId = orgId;
            currency.CreatedDate = DateTime.UtcNow;

            await context!.MerchantCurrencies!.AddAsync(currency);
            await context.SaveChangesAsync();

            return currency;
        }

        public async Task<MMerchantCurrency?> UpdateCurrencyById(string merchantCurrencyId, MMerchantCurrency currency)
        {
            Guid id = Guid.Parse(merchantCurrencyId);
            var existing = await context!.MerchantCurrencies!
                .AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.PayinFeePct = currency.PayinFeePct;
                existing.PayinMinAmount = currency.PayinMinAmount;
                existing.PayinMaxAmount = currency.PayinMaxAmount;
                existing.PayinDiscardCent = currency.PayinDiscardCent;
                existing.PayinIncludeGlobalBankAccount = currency.PayinIncludeGlobalBankAccount;
                existing.PayinWhitelistBankAccountNames = currency.PayinWhitelistBankAccountNames;
                existing.PayinRandomDecimal = currency.PayinRandomDecimal;
                existing.PayinDailyTxAmountLimit = currency.PayinDailyTxAmountLimit;
                existing.PayinDailyTxCountLimit = currency.PayinDailyTxCountLimit;
                existing.PayinExpireMinute = currency.PayinExpireMinute;

                existing.PayoutFeePct = currency.PayoutFeePct;
                existing.PayoutMinAmount = currency.PayoutMinAmount;
                existing.PayoutMaxAmount = currency.PayoutMaxAmount;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MMerchantCurrency?> GetCurrencyById(string merchantCurrencyId)
        {
            Guid id = Guid.Parse(merchantCurrencyId);
            var existing = await context!.MerchantCurrencies!
                .AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            return existing;
        }

        public async Task<MMerchantCurrency?> UpdateCurrencyStatusById(string merchantCurrencyId, string status)
        {
            Guid id = Guid.Parse(merchantCurrencyId);
            var existing = context!.MerchantCurrencies!
                .AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            
            if (existing != null)
            {
                existing.Status = status;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}