using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class CurrencyAccountService : BaseService, ICurrencyAccountService
    {
        private readonly ICurrencyAccountRepository? repository = null;
        private readonly IPointRepository? _pointRepo = null;
        private readonly List<MBank> _banks;
        private readonly List<MCryptoCurrency> _cryptoCurrencies;
        private readonly IRedisHelper _redis;

        public CurrencyAccountService(ICurrencyAccountRepository repo, IPointRepository pointRepo, IRedisHelper redis) : base()
        {
            repository = repo;
            _pointRepo = pointRepo;
            _redis = redis;

            _cryptoCurrencies = [
                new() { Code = "BTC", Name = "Bitcoin", DefaultNetwork = "BITCOIN", DefaultDecimal = 8 },
                new() { Code = "ETH", Name = "Ethereum", DefaultNetwork = "ETHEREUM", DefaultDecimal = 18 },
                new() { Code = "USDT", Name = "Tether", DefaultNetwork = "TRON", DefaultDecimal = 6, IsToken = true },
                new() { Code = "USDC", Name = "USD Coin", DefaultNetwork = "ETHEREUM", DefaultDecimal = 6, IsToken = true },
                new() { Code = "BNB", Name = "BNB", DefaultNetwork = "BSC", DefaultDecimal = 18 },
                new() { Code = "XRP", Name = "Ripple", DefaultNetwork = "RIPPLE", DefaultDecimal = 6 },
                new() { Code = "SOL", Name = "Solana", DefaultNetwork = "SOLANA", DefaultDecimal = 9 },
                new() { Code = "ADA", Name = "Cardano", DefaultNetwork = "CARDANO", DefaultDecimal = 6 },
                new() { Code = "DOGE", Name = "Dogecoin", DefaultNetwork = "DOGECOIN", DefaultDecimal = 8 },
                new() { Code = "TRX", Name = "TRON", DefaultNetwork = "TRON", DefaultDecimal = 6 },
                new() { Code = "TON", Name = "Toncoin", DefaultNetwork = "TON", DefaultDecimal = 9 },
                new() { Code = "DOT", Name = "Polkadot", DefaultNetwork = "POLKADOT", DefaultDecimal = 10 },
                new() { Code = "MATIC", Name = "Polygon", DefaultNetwork = "POLYGON", DefaultDecimal = 18 },
                new() { Code = "LTC", Name = "Litecoin", DefaultNetwork = "LITECOIN", DefaultDecimal = 8 },
                new() { Code = "KAS", Name = "Kaspa", DefaultNetwork = "KASPA", DefaultDecimal = 8 },
                new() { Code = "AVAX", Name = "Avalanche", DefaultNetwork = "AVALANCHE", DefaultDecimal = 18 },
                new() { Code = "SHIB", Name = "Shiba Inu", DefaultNetwork = "ETHEREUM", DefaultDecimal = 18, IsToken = true },
                new() { Code = "LINK", Name = "Chainlink", DefaultNetwork = "ETHEREUM", DefaultDecimal = 18, IsToken = true },
                new() { Code = "ATOM", Name = "Cosmos", DefaultNetwork = "COSMOS", DefaultDecimal = 6 },
                new() { Code = "XLM", Name = "Stellar", DefaultNetwork = "STELLAR", DefaultDecimal = 7 },
            ];

            _banks = [
                new() 
                { 
                    BankCode = "PP", 
                    BankNameTh = "บริการพร้อมเพย์", 
                    BankNameEng = "Prompt Pay",
                    Type = "PromptPay",
                    QrSupportFlag = true,
                },

                new() 
                { 
                    BankCode = "BAY", 
                    BankNameTh = "ธนาคารกรุงศรีอยุธยา", 
                    BankNameEng = "Bank of Ayudhya",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new () 
                { 
                    BankCode = "KBANK", 
                    BankNameTh = "ธนาคารกสิกรไทย", 
                    BankNameEng = "Kasikorn Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "KTB",
                    BankNameTh = "ธนาคารกรุงไทย",
                    BankNameEng = "Krung Thai Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "SCB", 
                    BankNameTh = "ธนาคารไทยพาณิชย์", 
                    BankNameEng = "Siam Commercial Bank",
                    Type = "Native",
                    QrSupportFlag = true,  
                },

                new ()
                { 
                    BankCode = "BBL", 
                    BankNameTh = "ธนาคารกรุงเทพ", 
                    BankNameEng = "Bank of Bangkok",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new () 
                { 
                    BankCode = "TMB", 
                    BankNameTh = "ธนาคารทหารไทย", 
                    BankNameEng = "Bank of Thailand",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "GSB", 
                    BankNameTh = "ธนาคารออมสิน", 
                    BankNameEng = "Government Savings Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                {
                    BankCode = "UOB", 
                    BankNameTh = "ธนาคารยูโอบี", 
                    BankNameEng = "United Overseas Bank",
                    Type = "Native",
                    QrSupportFlag = false,                
                },

                new ()
                { 
                    BankCode = "CIMBT", 
                    BankNameTh = "ธนาคารซีไอเอ็มบี ไทย", 
                    BankNameEng = "Citibank (Thailand)",
                    Type = "Native",
                    QrSupportFlag = false, 
                },

                new ()
                {
                    BankCode = "SCBT", 
                    BankNameTh = "ธนาคารสแตนดาร์ดชาร์เตอร์ด (ไทย)", 
                    BankNameEng = "Standard Chartered Bank (Thailand)",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "TISCO", 
                    BankNameTh = "ธนาคารทิสโก้", 
                    BankNameEng = "Tisco Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "LHFG", 
                    BankNameTh = "ธนาคารแลนด์ แอนด์ เฮ้าส์", 
                    BankNameEng = "Land and Houses Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },
                
                new ()
                {
                    BankCode = "ICBC",
                    BankNameTh = "ธนาคารไอซีบีซี (ไทย)", 
                    BankNameEng = "Industrial and Commercial Bank of China (Thailand)",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "CITI", 
                    BankNameTh = "ธนาคารซิตี้แบงก์", 
                    BankNameEng = "Citibank",
                    Type = "Native",
                    QrSupportFlag = false,  
                },

                new ()
                { 
                    BankCode = "EXIM", 
                    BankNameTh = "ธนาคารเพื่อการส่งออกและนำเข้าแห่งประเทศไทย", 
                    BankNameEng = "Export-Import_BANK_OF_THAILAND", 
                    Type = "Native",
                    QrSupportFlag = false,  
                }
            ];
        }

        private async Task<MTxBalance> GetCurrencyAccountCurrentDailyTxBalance(string orgId, string currencyAccountId)
        {
            var r = new MTxBalance()
            {
                TxCount = 0,
                TxAmount = 0
            };

            var key = CacheHelper.CreatePayInBankAccountDailyTxKey(orgId, currencyAccountId);
            var cacheValue = await _redis.GetObjectAsync<MTxBalance>(key);
            if (cacheValue != null)
            {
                r = cacheValue;
            }

            return r;
        }

        private bool IsNativeQrSupport(MCurrencyAccount currencyAccount)
        {
            var bankCode = currencyAccount.BankCode;

            foreach (var bnk in _banks)
            {
                if ((bnk.BankCode == bankCode) && (bnk.Type == "Native") && currencyAccount.BankAccountType == "Native")
                {
                    return bnk.QrSupportFlag;
                }
            }
            return false;
        }


        public async Task<MVCurrencyAccount> GetCurrencyAccountById(string orgId, string currencyAccountId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCurrencyAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(currencyAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Currency Account ID [{currencyAccountId}] format is invalid";

                return r;
            }

            var result = await repository!.GetCurrencyAccountById(currencyAccountId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Currency Account ID [{currencyAccountId}] not found for the organization [{orgId}]";

                return r;
            }

            var bc = result.BankConfig;
            if (!string.IsNullOrEmpty(bc))
            {
                var obj = JsonSerializer.Deserialize<MBankAccountConfig>(bc);
                result.BankConfigObj = obj;                
            }

            // ดึงข้อมูลจาก cache
            var currentDailyTxBalance = await GetCurrencyAccountCurrentDailyTxBalance("global", currencyAccountId);
            result.CurrentDailyTxAmount = currentDailyTxBalance.TxAmount;

            result.BankIsNativeQrSupport = IsNativeQrSupport(result);

            r.CurrencyAccount = result;
            r.CurrencyAccount.BankConfig = "";

            return r;
        }

        public async Task<MVCurrencyAccount> AddFiatCurrencyAccount(string orgId, MCurrencyAccount currencyAccount)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCurrencyAccount()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(currencyAccount.Currency))
            {
                r.Status = "CURRENCY_CODE_MISSING";
                r.Description = $"Bank Code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.BankCode))
            {
                r.Status = "BANK_CODE_MISSING";
                r.Description = $"Bank Code is missing!!!";

                return r;
            }

            var cat = currencyAccount.AccountType; //PayIn, PayOut

            if (string.IsNullOrEmpty(currencyAccount.AccountType))
            {
                r.Status = "CURRENCY_ACCOUNT_TYPE_MISSING";
                r.Description = $"Currency account type is missing!!!";

                return r;
            }

            if ((cat != "PayIn") && (cat != "PayOut") && (cat != "Transit"))
            {
                r.Status = "CURRENCY_ACCOUNT_TYPE_INVALID";
                r.Description = $"Currency account type must be PayIn or PayOut or Transit !!!";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.BankAccountName))
            {
                r.Status = "ACCOUNT_NAME_MISSING";
                r.Description = $"Bank Account name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.BankAccountNo))
            {
                r.Status = "ACCOUNT_NUMBER_MISSING";
                r.Description = $"Bank Account number is missing!!!";

                return r;
            }

            var isAccountNoExist = await repository!.IsFiatCurrencyAccountNoExist(currencyAccount.Currency, currencyAccount.BankCode, currencyAccount.BankAccountNo);
            if (isAccountNoExist)
            {
                r.Status = "ACCOUNT_NUMBER_DUPLICATE";
                r.Description = $"Bank Account number [{currencyAccount.BankAccountNo}] already exist!!!";

                return r;
            }

            var isNameExist = await repository!.IsFiatCurrencyAccountNameExist(currencyAccount.Currency, currencyAccount.BankCode, currencyAccount.BankAccountName);
            if (isNameExist)
            {
                r.Status = "ACCOUNT_NAME_DUPLICATE";
                r.Description = $"Bank Account name [{currencyAccount.BankAccountName}] already exist!!!";

                return r;
            }

            currencyAccount.Status = "Pending";
            var result = await repository!.AddCurrencyAccount(currencyAccount);

            if (result != null)
            {
                var bankAccountId = result.Id.ToString()!;

                //เพิ่ม wallet ให้อัตโนมัติ
                _pointRepo!.SetCustomOrgId(result.OrgId!); //ตรงนี้จะเป็น global
                var wallet = await _pointRepo!.GetWalletByRefId(bankAccountId);
                if (wallet == null)
                {
                    //ยังไม่เคยสร้าง wallet มาก่อนก็สร้างให้เลย
                    var w = new MWallet()
                    {
                        Name = $"{currencyAccount.Currency}:{bankAccountId}",
                        BankAccountId = bankAccountId,
                        PointBalance = 0,
                        PointBalanceDecimal = 0,
                        Tags = $"Currency={currencyAccount.Currency}, Id={bankAccountId}",
                        Description = $"Auto generated wallet for [{currencyAccount.Currency}] currency for [{result.BankAccountName}]",
                    };

                    var _ = await _pointRepo.AddWallet(w);
                }
            }

            r.CurrencyAccount = result;

            return r;
        }

        public List<MCryptoCurrency> GetAvailableCryptoCurrencies()
        {
            return _cryptoCurrencies;
        }

        public async Task<MVCurrencyAccount> AddCryptoCurrencyAccount(string orgId, MCurrencyAccount currencyAccount)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCurrencyAccount()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(currencyAccount.Currency))
            {
                r.Status = "CURRENCY_CODE_MISSING";
                r.Description = $"Currency code is missing!!!";

                return r;
            }

            var cat = currencyAccount.AccountType; //PayIn, PayOut, Transit

            if (string.IsNullOrEmpty(currencyAccount.AccountType))
            {
                r.Status = "CURRENCY_ACCOUNT_TYPE_MISSING";
                r.Description = $"Currency account type is missing!!!";

                return r;
            }

            if ((cat != "PayIn") && (cat != "PayOut") && (cat != "Transit"))
            {
                r.Status = "CURRENCY_ACCOUNT_TYPE_INVALID";
                r.Description = $"Currency account type must be PayIn or PayOut or Transit !!!";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.AccountLevel))
            {
                r.Status = "ACCOUNT_LEVEL_MISSING";
                r.Description = $"Account level is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.CryptoWalletNetwork))
            {
                r.Status = "CRYPTO_WALLET_NETWORK_MISSING";
                r.Description = $"Crypto wallet network is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.CryptoExtendedPublicKey))
            {
                r.Status = "CRYPTO_EXTENDED_PUBLIC_KEY_MISSING";
                r.Description = $"Crypto extended public key is missing!!!";

                return r;
            }

            var isEpkExist = await repository!.IsCrypotCurrencyEpkExist(currencyAccount.Currency, currencyAccount.CryptoExtendedPublicKey);
            if (isEpkExist)
            {
                r.Status = "CRYPTO_EXTENDED_PUBLIC_KEY_DUPLICATE";
                r.Description = $"Extended public key already exists for currency [{currencyAccount.Currency}]!!!";

                return r;
            }

            currencyAccount.CurrencyCategory = "CRYPTO";
            currencyAccount.Status = "Pending";
            var result = await repository!.AddCurrencyAccount(currencyAccount);

            if (result != null)
            {
                var currencyAccountId = result.Id.ToString()!;

                //เพิ่ม wallet ให้อัตโนมัติ
                _pointRepo!.SetCustomOrgId(result.OrgId!); //ตรงนี้จะเป็น global
                var wallet = await _pointRepo!.GetWalletByRefId(currencyAccountId);
                if (wallet == null)
                {
                    //ยังไม่เคยสร้าง wallet มาก่อนก็สร้างให้เลย
                    var w = new MWallet()
                    {
                        Name = $"{currencyAccount.Currency}:{currencyAccountId}",
                        BankAccountId = currencyAccountId,
                        PointBalance = 0,
                        PointBalanceDecimal = 0,
                        Tags = $"Currency={currencyAccount.Currency}, Id={currencyAccountId}",
                        Description = $"Auto generated wallet for [{currencyAccount.Currency}] crypto account",
                    };

                    var _ = await _pointRepo.AddWallet(w);
                }
            }

            r.CurrencyAccount = result;

            return r;
        }

        public async Task<List<MCurrencyAccount>> GetCurrencyAccounts(string orgId, VMCurrencyAccount param)
        {
            repository!.SetCustomOrgId(orgId);

            var merchantCountAggr = await repository.GetMerchantCountByCurrencyAccountId();
            var merchantCountDict = merchantCountAggr.ToDictionary(g => g.CurrencyAccountId!, g => g.MerchantCount);

            _pointRepo!.SetCustomOrgId(orgId);
            var balanceAggr = await _pointRepo.GetWalletBalancesGroupByBankAccountId();
            var balanceDict = balanceAggr.ToDictionary(g => $"{g.BankAccountId!}", g => g.PointBalanceDecimal);

            var accounts = await repository.GetCurrencyAccounts(param);

            foreach (var account in accounts)
            {
                var id = account.Id.ToString();

                if (!string.IsNullOrEmpty(id) && merchantCountDict.TryGetValue(id, out var merchantCount))
                {
                    account.MerchantLinkCount = merchantCount;
                }
                else
                {
                    account.MerchantLinkCount = 0;
                }

                if (account.AccountLevel == "Global")
                {
                    account.MerchantLinkCount = 99999; //เป็น global
                }

                account.CurrentWalletBalance = 0;
                if (!string.IsNullOrEmpty(id) && balanceDict.TryGetValue(id, out var currentWalletBalance))
                {
                    account.CurrentWalletBalance = currentWalletBalance;
                }

                account.BankConfig = "";
            }

            return accounts;
        }

        public async Task<int> GetCurrencyAccountCount(string orgId, VMCurrencyAccount param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetCurrencyAccountCount(param);

            return result;
        }

        public async Task<MVCurrencyAccount> UpdateCurrencyAccountById(string orgId, string currencyAccountId, MCurrencyAccount currencyAccount)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCurrencyAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(currencyAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Currency Account ID [{currencyAccountId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(currencyAccount.AccountLevel))
            {
                r.Status = "ACCOUNT_LEVEL_MISSING";
                r.Description = $"Account level is missing!!!";

                return r;
            }

            var result = await repository!.UpdateCurrencyAccountById(currencyAccountId, currencyAccount);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Currency Account ID [{currencyAccountId}] not found for the organization [{orgId}]";

                return r;
            }

            r.CurrencyAccount = result;

            return r;
        }

        public async Task<MVCurrencyAccount?> UpdateCurrencyAccountStatusById(string orgId, string currencyAccountId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCurrencyAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(currencyAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Currency Account ID [{currencyAccountId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateCurrencyAccountStatusById(currencyAccountId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Currency Account ID [{currencyAccountId}] not found";

                return r;
            }

            r.CurrencyAccount = result;

            return r;
        }

        public async Task<MVCurrencyAccount> DeleteCurrencyAccountById(string orgId, string currencyAccountId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCurrencyAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(currencyAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Currency Account ID [{currencyAccountId}] format is invalid";

                return r;
            }

            var result = await repository!.DeleteCurrencyAccountById(currencyAccountId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Currency Account ID [{currencyAccountId}] not found for the organization [{orgId}]";

                return r;
            }

            r.CurrencyAccount = result;

            return r;
        }

    }
}
