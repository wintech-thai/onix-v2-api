using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class CurrencyAcountRepository : BaseRepository, ICurrencyAcountRepository
    {
        public CurrencyAcountRepository(IDataContext ctx)
        {
            context = ctx;
        }

        //==== Fiat ====
        public async Task<bool> IsFiatCurrencyAccountNoExist(string currencyCode, string bankCode, string accountNo)
        {
            var exists = await context!.CurrencyAccounts!
                .AsExpandable()
                .AnyAsync(p => p!.BankAccountNo!.Equals(accountNo) &&
                    p!.BankCode!.Equals(bankCode) && 
                    p!.Currency!.Equals(currencyCode) && 
                    p!.OrgId!.Equals(orgId));

            return exists;
        }

        public async Task<bool> IsFiatCurrencyAccountNameExist(string currencyCode, string bankCode, string accountName)
        {
            var exists = await context!.CurrencyAccounts!
                .AsExpandable()
                .AnyAsync(p => p!.BankAccountName!.Equals(accountName) &&
                    p!.BankCode!.Equals(bankCode) && 
                    p!.Currency!.Equals(currencyCode) && 
                    p!.OrgId!.Equals(orgId));

            return exists;
        }

        public async Task<MCurrencyAccount?> GetFiatCurrencyAccountByAccountNo(string currencyCode, string bankCode, string accountNo)
        {
            var exists = await context!.CurrencyAccounts!
                .AsExpandable()
                .Where(p => p!.BankAccountNo!.Equals(accountNo) &&
                    p!.BankCode!.Equals(bankCode) && 
                    p!.Currency!.Equals(currencyCode) && 
                    p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();

            return exists;
        }

        public async Task<MCurrencyAccount?> GetFiatCurrencyAccountByAccountName(string currencyCode, string bankCode, string accountName)
        {
            var exists = await context!.CurrencyAccounts!
                .AsExpandable()
                .Where(p => p!.BankAccountName!.Equals(accountName) &&
                    p!.BankCode!.Equals(bankCode) && 
                    p!.Currency!.Equals(currencyCode) && 
                    p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();

            return exists;
        }

        public async Task<MCurrencyAccount?> UpdateCurrencyAccountBankConfigById(string currencyAccountId, string bankConfig)
        {
            Guid id = Guid.Parse(currencyAccountId);
            var existing = await context!.CurrencyAccounts!
                .AsExpandable()
                .Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.BankConfig = bankConfig;
            }

            await context.SaveChangesAsync();
            return existing;
        }


        //==== Common ====
        private ExpressionStarter<MCurrencyAccount> CurrencyAccountPredicate(VMCurrencyAccount param)
        {
            var pd = PredicateBuilder.New<MCurrencyAccount>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.CurrencyCode != null) && (param.CurrencyCode != ""))
            {
                var currencyCodePd = PredicateBuilder.New<MCurrencyAccount>();
                currencyCodePd = currencyCodePd.Or(p => p.Currency!.Equals(param.CurrencyCode));

                pd = pd.And(currencyCodePd);
            }

            if ((param.CurrencyCategory != null) && (param.CurrencyCategory != ""))
            {
                var ccPd = PredicateBuilder.New<MCurrencyAccount>();
                ccPd = ccPd.Or(p => p.CurrencyCategory!.Equals(param.CurrencyCategory));

                pd = pd.And(ccPd);
            }

            if ((param.AccountLevel != null) && (param.AccountLevel != ""))
            {
                var accLevelPd = PredicateBuilder.New<MCurrencyAccount>();
                accLevelPd = accLevelPd.Or(p => p.AccountLevel!.Equals(param.AccountLevel));

                pd = pd.And(accLevelPd);
            }

            if ((param.AccountType != null) && (param.AccountType != ""))
            {
                var accTypePd = PredicateBuilder.New<MCurrencyAccount>();
                accTypePd = accTypePd.Or(p => p.AccountType!.Equals(param.AccountType));

                pd = pd.And(accTypePd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MCurrencyAccount>();
                fullTextPd = fullTextPd.Or(p => p.BankAccountName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.BankCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.BankAccountNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.CryptoExtendedPublicKey!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.CryptoWalletNetwork!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<int> GetCurrencyAccountCount(VMCurrencyAccount param)
        {
            var predicate = CurrencyAccountPredicate(param!);
            var result = await context!.CurrencyAccounts!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<List<MCurrencyAccount>> GetCurrencyAccounts(VMCurrencyAccount param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = CurrencyAccountPredicate(param!);
            var result = await context!.CurrencyAccounts!.AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<List<MCurrencyAccount>> GetAllCurrencyAccounts(VMCurrencyAccount param)
        {
            var predicate = CurrencyAccountPredicate(param!);
            var result = await context!.CurrencyAccounts!.AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();

            return result;
        }

        public async Task<MCurrencyAccount?> DeleteCurrencyAccountById(string currencyAccountId)
        {
            Guid id = Guid.Parse(currencyAccountId);

            var existing = await context!.CurrencyAccounts!.AsExpandable()
            .Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId))
            .FirstOrDefaultAsync();

            if (existing != null)
            {
                context.CurrencyAccounts!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MCurrencyAccount?> GetCurrencyAccountById(string currencyAccountId)
        {
            Guid id = Guid.Parse(currencyAccountId);

            var existing = await context!.CurrencyAccounts!.AsExpandable()
            .Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId))
            .FirstOrDefaultAsync();

            return existing;
        }

        public async Task<MCurrencyAccount> AddCurrencyAccount(MCurrencyAccount currencyAccount)
        {
            currencyAccount.OrgId = orgId;
            currencyAccount.CreatedDate = DateTime.UtcNow;

            await context!.CurrencyAccounts!.AddAsync(currencyAccount);
            await context.SaveChangesAsync();

            return currencyAccount;
        }

        public async Task<MCurrencyAccount?> UpdateCurrencyAccountById(string currencyAccountId, MCurrencyAccount currencyAccount)
        {
            Guid id = Guid.Parse(currencyAccountId);
            var existing = await context!.CurrencyAccounts!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Tags = currencyAccount.Tags;
                existing.AccountLevel = currencyAccount.AccountLevel;
                existing.IsRandomCent = currencyAccount.IsRandomCent;
                existing.DecimalAction = currencyAccount.DecimalAction;

                existing.TxMinAmount = currencyAccount.TxMinAmount;
                existing.TxMaxAmount = currencyAccount.TxMaxAmount;
                existing.DailyTotalAmountLimit = currencyAccount.DailyTotalAmountLimit;
                existing.DailyTotalCountLimit = currencyAccount.DailyTotalCountLimit;
                existing.DecimalAction = currencyAccount.DecimalAction;
                existing.DecimalAction = currencyAccount.DecimalAction;
                existing.DecimalAction = currencyAccount.DecimalAction;
                existing.DecimalAction = currencyAccount.DecimalAction;
                

                //existing.CryptoWalletNetwork = currencyAccount.CryptoWalletNetwork;
                //existing.CryptoWalletType = currencyAccount.CryptoWalletType;
                //existing.CryptoExtendedPublicKey = currencyAccount.CryptoExtendedPublicKey;
                existing.CryptoDerivationPath = currencyAccount.CryptoDerivationPath;
                existing.CryptoQrScheme = currencyAccount.CryptoQrScheme;
                existing.CryptoAddressPrefix = currencyAccount.CryptoAddressPrefix;
                existing.CryptoTokenContract = currencyAccount.CryptoTokenContract;
                existing.CryptoDecimal = currencyAccount.CryptoDecimal; 
                existing.CryptoAddressBranch = currencyAccount.CryptoAddressBranch;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MCurrencyAccount?> UpdateCurrencyAccountStatusById(string currencyAccountId, string status)
        {
            Guid id = Guid.Parse(currencyAccountId);
            var existing = context!.CurrencyAccounts!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            if (existing != null)
            {
                existing.Status = status;
            }

            await context.SaveChangesAsync();
            return existing;
        }


        //===== CRYPTO ====
        public async Task<bool> IsCrypotCurrencyEpkExist(string currencyCode, string epk)
        {
            var exists = await context!.CurrencyAccounts!
                .AsExpandable()
                .AnyAsync(p => p!.CryptoExtendedPublicKey!.Equals(epk) && 
                    p!.Currency!.Equals(currencyCode) && 
                    p!.OrgId!.Equals(orgId));

            return exists;
        }

        public async Task<MCurrencyAccount?> GetCryptoCurrencyAccountByEpk(string currencyCode, string epk)
        {
            var exists = await context!.CurrencyAccounts!
                .AsExpandable()
                .Where(p => p!.CryptoExtendedPublicKey!.Equals(epk) && 
                    p!.Currency!.Equals(currencyCode) && 
                    p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();

            return exists;
        }

        //==== Merchant Binding ====
        public async Task<MCurrencyAccountMerchant?> SelectMerchant(string currencyAccountId, string merchantId)
        {
            //ให้ทำการเพิ่ม row ไปที่ MCurrencyAccountMerchant โดยมีค่า currencyAccountId, merchantId และ OrgId ที่ตรงกับ orgId ที่ส่งมา
            //ถ้ามีอยู่แล้วไม่ต้องทำอะไร
            var existing = await context!.CurrencyAccountMerchants!.AsExpandable()
                .Where(p => p!.CurrencyAccountId!.Equals(currencyAccountId) 
                && p!.MerchantId!.Equals(merchantId) 
                && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                return existing;
            }

            var ca = await GetCurrencyAccountById(currencyAccountId);
            if (ca == null)
            {
                return null;
            }

            // ถ้าไม่มีอยู่ ให้สร้างใหม่
            var newMerchant = new MCurrencyAccountMerchant
            {
                Id = Guid.NewGuid(),
                CurrencyAccountId = currencyAccountId,
                MerchantId = merchantId,
                Currency = ca.Currency,
                CurrencyCategory = ca.CurrencyCategory, //Fiat, Crypto
                AccountCategory = ca.AccountType, //PayIn, PayOut
                OrgId = orgId
            };

            context.CurrencyAccountMerchants!.Add(newMerchant);
            await context.SaveChangesAsync();

            return newMerchant;
        }

        public async Task<MCurrencyAccountMerchant?> UnSelectMerchant(string currencyAccountId, string merchantId)
        {
            //ให้ทำการเพิ่ม row ไปที่ MCurrencyAccountMerchant โดยมีค่า currencyAccountId, merchantId และ OrgId ที่ตรงกับ orgId ที่ส่งมา
            //ถ้ามีอยู่แล้วไม่ต้องทำอะไร
            var existing = await context!.CurrencyAccountMerchants!.AsExpandable()
                .Where(p => p!.CurrencyAccountId!.Equals(currencyAccountId) 
                && p!.MerchantId!.Equals(merchantId) 
                && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                context.CurrencyAccountMerchants!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<List<MCurrencyAccountMerchant>> GetMerchantCountByCurrencyAccountId()
        {
            var result = await context!.CurrencyAccountMerchants!.AsExpandable()
                .GroupBy(x => x.CurrencyAccountId)
                .Select(g => new MCurrencyAccountMerchant()
                {
                    CurrencyAccountId = g.Key,
                    MerchantCount = g.Count()
                })
                .ToListAsync();

            return result;
        }

        public IQueryable<MCurrencyAccountMerchant> GetSelectionV2()
        {
            var query =
                from bam in context!.BankAccountMerchants

                join ba in context.CurrencyAccounts!
                    on bam.BankAccountId equals ba.Id.ToString() into bankAccounts
                from bankaccount in bankAccounts.DefaultIfEmpty()

                join mc in context.Merchants!
                    on bam.MerchantId equals mc.Id.ToString() into merchants
                from merchant in merchants.DefaultIfEmpty()

                select new { bam, bankaccount, merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MCurrencyAccountMerchant
            {
                Id = x.bam.Id,
                OrgId = x.bam.OrgId,
                CurrencyAccountId = x.bam.BankAccountId,
                Currency = x.bankaccount.Currency,
                MerchantId = x.bam.MerchantId,
                CreatedDate = x.bam.CreatedDate,

                //ข้อมูลของ Bank Account
                BankCode = x.bankaccount != null ? x.bankaccount.BankCode : null,
                AccountNumber = x.bankaccount != null ? x.bankaccount.BankAccountNo : null,
                AccountName = x.bankaccount != null ? x.bankaccount.BankAccountName : null,
                AccountType = x.bankaccount != null ? x.bankaccount.AccountType : null,
                AccountLevel = x.bankaccount != null ? x.bankaccount.AccountLevel : null,
                BankAccountStatus = x.bankaccount != null ? x.bankaccount.Status : null,

                //ข้อมูลของ Merchant
                MerchantCode = x.merchant != null ? x.merchant.Code : null,
                MerchantName = x.merchant != null ? x.merchant.Name : null,
                MerchantStatus = x.merchant != null ? x.merchant.Status : null,
            });
        }

        public async Task<List<MCurrencyAccountMerchant>> GetCurrencyAccountCountByMerchantId()
        {
            var result = await GetSelectionV2().AsExpandable()
                .GroupBy(x => new{ x.MerchantId, x.AccountCategory })
                .Select(g => new MCurrencyAccountMerchant()
                {
                    MerchantId = g.Key.MerchantId,
                    AccountCategory = g.Key.AccountCategory,
                    BankAccountCount = g.Count()
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MCurrencyAccountMerchant>> GetCurrencyAccountsForMerchant(string merchantId, string accountType)
        {
            var result = await GetSelectionV2().AsExpandable()
                .Where(p => p.MerchantId == merchantId && p.AccountCategory == accountType)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<List<MCurrencyAccountMerchant>> GetMerchantsForCurrencyAccount(string currencyAccountId)
        {
            var result = await GetSelectionV2().AsExpandable()
                .Where(p => p.CurrencyAccountId == currencyAccountId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<List<MCurrencyAccountMerchant>> GetSelectedCurrencyAccounts(string accountType)
        {
            var result = await GetSelectionV2().AsExpandable()
                .Where(p => p.AccountCategory == accountType)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }
    }
}