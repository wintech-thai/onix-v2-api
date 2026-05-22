using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class BankAccountRepository : BaseRepository, IBankAccountRepository
    {
        public BankAccountRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsBankAccountNoExist(string accountNo)
        {
            var exists = await context!.BankAccounts!.AsExpandable().AnyAsync(p => p!.AccountNumber!.Equals(accountNo) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<bool> IsBankAccountNameExist(string bankCode, string accountName)
        {
            var exists = await context!.BankAccounts!.AsExpandable()
                .AnyAsync(p => p!.BankCode!.Equals(bankCode) && p!.AccountName!.Equals(accountName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MBankAccount?> GetBankAccountByAccountNo(string accountNo)
        {
            var exists = await context!.BankAccounts!.AsExpandable()
                .Where(p => p!.AccountNumber!.Equals(accountNo) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<MBankAccount?> GetBankAccountByAccountName(string bankCode, string accountName)
        {
            var exists = await context!.BankAccounts!.AsExpandable()
                .Where(p => p!.BankCode!.Equals(bankCode) && p!.AccountName!.Equals(accountName) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }


        //=== Start V2 ===
        public IQueryable<MBankAccountMerchant> GetSelectionV2()
        {
            var query =
                from bam in context!.BankAccountMerchants

                join ba in context.BankAccounts!
                    on bam.BankAccountId equals ba.Id.ToString() into bankAccounts
                from bankaccount in bankAccounts.DefaultIfEmpty()

                join mc in context.Merchants!
                    on bam.MerchantId equals mc.Id.ToString() into merchants
                from merchant in merchants.DefaultIfEmpty()

                select new { bam, bankaccount, merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MBankAccountMerchant
            {
                Id = x.bam.Id,
                OrgId = x.bam.OrgId,
                BankAccountId = x.bam.BankAccountId,
                MerchantId = x.bam.MerchantId,
                CreatedDate = x.bam.CreatedDate,

                //ข้อมูลของ Bank Account
                BankCode = x.bankaccount != null ? x.bankaccount.BankCode : null,
                AccountNumber = x.bankaccount != null ? x.bankaccount.AccountNumber : null,
                AccountName = x.bankaccount != null ? x.bankaccount.AccountName : null,
                PromptPayId = x.bankaccount != null ? x.bankaccount.PromptPayId : null,
                AccountType = x.bankaccount != null ? x.bankaccount.AccountType : null,
                AccountCategory = x.bankaccount != null ? x.bankaccount.AccountCategory : null,
                AccountLevel = x.bankaccount != null ? x.bankaccount.AccountLevel : null,
                BankAccountStatus = x.bankaccount != null ? x.bankaccount.Status : null,

                //ข้อมูลของ Merchant
                MerchantCode = x.merchant != null ? x.merchant.Code : null,
                MerchantName = x.merchant != null ? x.merchant.Name : null,
                MerchantStatus = x.merchant != null ? x.merchant.Status : null,
            });
        }

        public async Task<List<MBankAccountMerchant>> GetMerchantsForBankAccount(string bankAccountId)
        {
            var result = await GetSelectionV2().AsExpandable()
                .Where(p => p.BankAccountId == bankAccountId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<List<MBankAccountMerchant>> GetMerchantCountByBankAccountId()
        {
            var result = await context!.BankAccountMerchants!.AsExpandable()
                .GroupBy(x => x.BankAccountId)
                .Select(g => new MBankAccountMerchant()
                {
                    BankAccountId = g.Key,
                    MerchantCount = g.Count()
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MBankAccountMerchant>> GetBankAccountCountByMerchantId()
        {
            var result = await GetSelectionV2().AsExpandable()
                .GroupBy(x => new{ x.MerchantId, x.AccountCategory })
                .Select(g => new MBankAccountMerchant()
                {
                    MerchantId = g.Key.MerchantId,
                    AccountCategory = g.Key.AccountCategory,
                    BankAccountCount = g.Count()
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MBankAccountMerchant>> GetPayInBankAccountsForMerchant(string merchantId)
        {
            var result = await GetSelectionV2().AsExpandable()
                .Where(p => p.MerchantId == merchantId && p.AccountCategory == "PayIn")
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<List<MBankAccountMerchant>> GetPayOutBankAccountsForMerchant(string merchantId)
        {
            var result = await GetSelectionV2().AsExpandable()
                .Where(p => p.MerchantId == merchantId && p.AccountCategory == "PayOut")
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<MBankAccountMerchant?> SelectMerchant(string bankAccountId, string merchantId)
        {
            //ให้ทำการเพิ่ม row ไปที่ MBankAccountMerchant โดยมีค่า bankAccountId, merchantId และ OrgId ที่ตรงกับ orgId ที่ส่งมา
            //ถ้ามีอยู่แล้วไม่ต้องทำอะไร
            var existing = await context!.BankAccountMerchants!.AsExpandable()
                .Where(p => p!.BankAccountId!.Equals(bankAccountId) 
                && p!.MerchantId!.Equals(merchantId) 
                && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            if (existing != null)
            {
                return existing;
            }

            // ถ้าไม่มีอยู่ ให้สร้างใหม่
            var newMerchant = new MBankAccountMerchant
            {
                Id = Guid.NewGuid(),
                BankAccountId = bankAccountId,
                MerchantId = merchantId,
                OrgId = orgId
            };

            context.BankAccountMerchants!.Add(newMerchant);
            await context.SaveChangesAsync();

            return newMerchant;
        }

        public async Task<MBankAccountMerchant?> UnSelectMerchant(string bankAccountId, string merchantId)
        {
            //ให้ทำการลบ row ออกจาก MBankAccountMerchant โดยมีค่า bankAccountId, merchantId และ OrgId ที่ตรงกับ orgId ที่ส่งมา
            var existing = await context!.BankAccountMerchants!.AsExpandable()
                .Where(p => p!.BankAccountId!.Equals(bankAccountId) 
                && p!.MerchantId!.Equals(merchantId) 
                && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
                
            if (existing != null)
            {
                context.BankAccountMerchants!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<List<MBankAccount>> GetBankAccounts(VMBankAccount param)
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

            var predicate = BankAccountPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<List<MBankAccount>> GetAllBankAccounts(VMBankAccount param)
        {
            var predicate = BankAccountPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetBankAccountCount(VMBankAccount param)
        {
            var predicate = BankAccountPredicate(param!);
            var result = await context!.BankAccounts!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MBankAccount?> GetBankAccountById(string bankAccountId)
        {
            Guid id = Guid.Parse(bankAccountId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MBankAccount> GetSelection()
        {
            var query =
                from ba in context!.BankAccounts!
                select new { ba };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MBankAccount
            {
                Id = x.ba.Id,
                OrgId = x.ba.OrgId,
                AccountNumber = x.ba.AccountNumber,
                AccountName = x.ba.AccountName,
                BankCode = x.ba.BankCode,
                CreatedDate = x.ba.CreatedDate,
                LastUsedDate = x.ba.LastUsedDate,
                Tags = x.ba.Tags,
                AccountType = x.ba.AccountType,
                AccountCategory = x.ba.AccountCategory,
                AccountLevel = x.ba.AccountLevel,
                PayinMinAmount = x.ba.PayinMinAmount,
                PayinMaxAmount = x.ba.PayinMaxAmount,
                PayoutMinAmount = x.ba.PayoutMinAmount,
                PayoutMaxAmount = x.ba.PayoutMaxAmount,
                PromptPayId = x.ba.PromptPayId,
                DailyQuota = x.ba.DailyQuota,
                CurrentDailyPayinAmount = x.ba.CurrentDailyPayinAmount,
                CurrentDailyPayinCount = x.ba.CurrentDailyPayinCount,
                CurrentBalance = x.ba.CurrentBalance,
                DailyPayinCountQuota = x.ba.DailyPayinCountQuota,
                Status = x.ba.Status
            });
        }

        private ExpressionStarter<MBankAccount> BankAccountPredicate(VMBankAccount param)
        {
            var pd = PredicateBuilder.New<MBankAccount>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.AccountCategory != null) && (param.AccountCategory != ""))
            {
                var accCatPd = PredicateBuilder.New<MBankAccount>();
                accCatPd = accCatPd.Or(p => p.AccountCategory!.Equals(param.AccountCategory));

                pd = pd.And(accCatPd);
            }

            if ((param.AccountLevel != null) && (param.AccountLevel != ""))
            {
                var accLevelPd = PredicateBuilder.New<MBankAccount>();
                accLevelPd = accLevelPd.Or(p => p.AccountLevel!.Equals(param.AccountLevel));

                pd = pd.And(accLevelPd);
            }

            if ((param.AccountType != null) && (param.AccountType != ""))
            {
                var accTypePd = PredicateBuilder.New<MBankAccount>();
                accTypePd = accTypePd.Or(p => p.AccountType!.Equals(param.AccountType));

                pd = pd.And(accTypePd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MBankAccount>();
                fullTextPd = fullTextPd.Or(p => p.AccountName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.BankCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.AccountNumber!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PromptPayId!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MBankAccount> AddBankAccount(MBankAccount bankAccount)
        {
            bankAccount.OrgId = orgId;
            bankAccount.CreatedDate = DateTime.UtcNow;

            await context!.BankAccounts!.AddAsync(bankAccount);
            await context.SaveChangesAsync();

            return bankAccount;
        }

        public async Task<MBankAccount?> DeleteBankAccountById(string bankAccountId)
        {
            Guid id = Guid.Parse(bankAccountId);
            var existing = await context!.BankAccounts!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.BankAccounts!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MBankAccount?> UpdateBankAccountById(string bankAccountId, MBankAccount bankAccount)
        {
            Guid id = Guid.Parse(bankAccountId);
            var existing = await context!.BankAccounts!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.AccountName = bankAccount.AccountName;
                existing.AccountNumber = bankAccount.AccountNumber;
                existing.PromptPayId = bankAccount.PromptPayId;
                existing.AccountType = bankAccount.AccountType;
                existing.AccountLevel = bankAccount.AccountLevel;
                existing.Tags = bankAccount.Tags;
                existing.PayinMinAmount = bankAccount.PayinMinAmount;
                existing.PayinMaxAmount = bankAccount.PayinMaxAmount; 
                existing.PayoutMinAmount = bankAccount.PayoutMinAmount;
                existing.PayoutMaxAmount = bankAccount.PayoutMaxAmount;
                existing.DailyQuota = bankAccount.DailyQuota;
                existing.DailyPayinCountQuota = bankAccount.DailyPayinCountQuota;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MBankAccount?> UpdateBankAccountStatusById(string bankAccountId, string status)
        {
            Guid id = Guid.Parse(bankAccountId);
            var existing = context!.BankAccounts!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            if (existing != null)
            {
                existing.Status = status;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}