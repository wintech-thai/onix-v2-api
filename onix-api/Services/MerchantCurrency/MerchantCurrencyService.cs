using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class MerchantCurrencyService : BaseService, IMerchantCurrencyService
    {
        private readonly IMerchantCurrencyRepository? repository = null;
        private readonly IPointRepository? _pointRepo = null;
        //private readonly IRedisHelper _redis;

        private readonly List<MCurrency> _currencies =
        [
            new()
            {
                CurrencyCoode = "THB",
                CurrencyName = "Thai baht",
                Category = "FIAT",
            },
            new()
            {
                CurrencyCoode = "KAS",
                CurrencyName = "Kaspa crypto currency",
                Category = "CRYPTO",
            },
        ];

        public List<MCurrency> GetAvailableCurrencies(string category)
        {
            var currencies = _currencies
                .Where(b => b.Category!.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return currencies;
        }

        public MerchantCurrencyService(IMerchantCurrencyRepository repo, IPointRepository pointRepo) : base()
        {
            repository = repo;
            _pointRepo = pointRepo;
            //_redis = redis;

            _currencies = [
                new()
                {
                    CurrencyCoode = "THB", 
                    CurrencyName = "Thai baht", 
                    Category = "FIAT",
                },

                new() 
                { 
                    CurrencyCoode = "KAS", 
                    CurrencyName = "Kaspa crypto currency", 
                    Category = "CRYPTO",
                },
            ];
        }

        public async Task<MVMerchantCurrency?> GetCurrencyById(string orgId, string merchantCurrencyId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMerchantCurrency()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantCurrencyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant Currency ID [{merchantCurrencyId}] format is invalid";

                return r;
            }

            var result = await repository!.GetCurrencyById(merchantCurrencyId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant Currency ID [{merchantCurrencyId}] not found for the organization [{orgId}]";

                return r;
            }

            //TODO : Get current balance from cache

            r.MerchantCurrency = result;

            return r;
        }

        public async Task<MVMerchantCurrency> AddCurrency(string orgId, MMerchantCurrency currency)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMerchantCurrency()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(currency.Currency))
            {
                r.Status = "CURRENCY_MISSING";
                r.Description = $"Currency Code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(currency.MerchantId))
            {
                r.Status = "MERCHANT_ID_MISSING";
                r.Description = $"Merchant ID is missing!!!";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(currency.MerchantId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant Currency ID [{currency.MerchantId}] format is invalid";

                return r;
            }

            //Check ว่าเป็น known currency code หรือไม่
            var _currencyMap = _currencies.ToDictionary(x => x.CurrencyCoode!, StringComparer.OrdinalIgnoreCase);

            if (!_currencyMap.TryGetValue(currency.Currency, out var currencyCfg))
            {
                r.Status = "UNKNOWN_CURRENCY";
                r.Description = $"Currency ID [{currency.Currency}] is unknown";

                return r;
            }

            currency.CurrencyCategory = currencyCfg.Category;
            currency.CurrencyName = currencyCfg.CurrencyName;

            var isCurrencyExist = await repository!.IsCurrencyExist(currency.MerchantId, currency.Currency);
            if (isCurrencyExist)
            {
                r.Status = "CURRENCY_CODE_DUPLICATE";
                r.Description = $"Currency code [{currency.Currency}] already exist!!!";

                return r;
            }

            var w = new MWallet();

            currency.Status = "Active";
            currency.WalletId = w.Id.ToString();
            var result = await repository!.AddCurrency(currency);

            if (result != null)
            {
                var mcCurrencyId = result.Id.ToString()!;

                //เพิ่ม wallet ให้อัตโนมัติ
                _pointRepo!.SetCustomOrgId(result.OrgId!); //ตรงนี้จะเป็น global

                //ยังไม่เคยสร้าง wallet มาก่อนก็สร้างให้เลย
                w.Name = $"{currency.Currency}:{mcCurrencyId}";
                w.RefId = mcCurrencyId;
                w.PointBalance = 0;
                w.PointBalanceDecimal = 0;
                w.Tags = $"Currency={currency.Currency}, Merchant={currency.MerchantId}";
                w.Description = $"Auto generated wallet for [{currency.Currency}] currency for merchant [{currency.MerchantId}]";

                var _ = await _pointRepo.AddWallet(w);
            }

            r.MerchantCurrency = result;

            return r;
        }

        public async Task<List<MMerchantCurrency>> GetCurrenciesByMerchantId(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);

            var result = await repository.GetCurrenciesByMerchantId(merchantId);
            return result;
        }

        public async Task<MVMerchantCurrency> UpdateCurrencyById(string orgId, string merchantCurrencyId, MMerchantCurrency currency)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVMerchantCurrency()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantCurrencyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant Currency ID [{merchantCurrencyId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateCurrencyById(merchantCurrencyId, currency);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant Currency ID [{merchantCurrencyId}] not found";

                return r;
            }

            r.MerchantCurrency = result;

            return r;
        }

        public async Task<MVMerchantCurrency?> UpdateCurrencyStatusById(string orgId, string merchantCurrencyId, string status)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVMerchantCurrency()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantCurrencyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant Currency ID [{merchantCurrencyId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateCurrencyStatusById(merchantCurrencyId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant Currency ID [{merchantCurrencyId}] not found";

                return r;
            }

            r.MerchantCurrency = result;

            return r;

        }
    }
}
