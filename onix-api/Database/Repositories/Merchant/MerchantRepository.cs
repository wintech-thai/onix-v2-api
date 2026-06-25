using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class MerchantRepository : BaseRepository, IMerchantRepository
    {
        public MerchantRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsMerchantCodeExist(string code)
        {
            var exists = await context!.Merchants!.AsExpandable().AnyAsync(p => p!.Code!.Equals(code));
            return exists;
        }

        public async Task<bool> IsMerchantNameExist(string name)
        {
            var exists = await context!.Merchants!.AsExpandable().AnyAsync(p => p!.Name!.Equals(name));
            return exists;
        }

        public async Task<MMerchant?> GetMerchantByCode(string code)
        {
            var exists = await context!.Merchants!.AsExpandable().Where(p => p!.Code!.Equals(code)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<MMerchant?> GetMerchantByName(string name)
        {
            var exists = await context!.Merchants!.AsExpandable().Where(p => p!.Name!.Equals(name)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<MMerchant?> GetMerchantById(string merchantId)
        {
            Guid id = Guid.Parse(merchantId);
            var exists = await context!.Merchants!.AsExpandable().Where(p => p!.Id!.Equals(id)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<List<MMerchant>> GetMerchants(VMMerchant param)
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

            var predicate = MerchantPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetMerchantCount(VMMerchant param)
        {
            var predicate = MerchantPredicate(param!);
            var result = await context!.Merchants!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public IQueryable<MMerchant> GetSelection()
        {
            var query =
                from merchant in context!.Merchants
                select new { merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MMerchant
            {
                Id = x.merchant.Id,
                OrgId = x.merchant.OrgId,
                Code = x.merchant.Code,
                Name = x.merchant.Name,
                ContactEmail = x.merchant.ContactEmail,
                Tags = x.merchant.Tags,
                ContactPhone = x.merchant.ContactPhone,
                PayinFeePct = x.merchant.PayinFeePct,
                PayoutFeePct = x.merchant.PayoutFeePct,
                PayinMinAmount = x.merchant.PayinMinAmount,
                PayinMaxAmount = x.merchant.PayinMaxAmount,
                PayoutMinAmount = x.merchant.PayoutMinAmount,
                PayoutMaxAmount = x.merchant.PayoutMaxAmount,
                Status = x.merchant.Status,
                Description = x.merchant.Description,
                CreatedDate = x.merchant.CreatedDate,
                DiscardCent = x.merchant.DiscardCent,
                IncludeGlobalBankAccount = x.merchant.IncludeGlobalBankAccount,
            });
        }

        private ExpressionStarter<MMerchant> MerchantPredicate(VMMerchant param)
        {
            var pd = PredicateBuilder.New<MMerchant>(true);

            if ((param.Status != null) && (param.Status != ""))
            {
                var merchantPd = PredicateBuilder.New<MMerchant>();
                merchantPd = merchantPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(merchantPd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MMerchant>();
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ContactEmail!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ContactPhone!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MMerchant> AddMerchant(MMerchant merchant)
        {
            merchant.OrgId = orgId;
            merchant.CreatedDate = DateTime.UtcNow;

            await context!.Merchants!.AddAsync(merchant);
            await context.SaveChangesAsync();

            return merchant;
        }

        public async Task<MMerchant?> DeleteMerchantById(string merchantId)
        {
            Guid id = Guid.Parse(merchantId);
            var existing = await context!.Merchants!.AsExpandable().Where(p => p!.Id!.Equals(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.Merchants!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MMerchant?> UpdateMerchantById(string merchantId, MMerchant merchant)
        {
            Guid id = Guid.Parse(merchantId);
            var existing = await context!.Merchants!.AsExpandable().Where(p => p!.Id!.Equals(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Code = merchant.Code;
                existing.Name = merchant.Name;
                existing.ContactEmail = merchant.ContactEmail;
                existing.ContactPhone = merchant.ContactPhone;
                existing.Tags = merchant.Tags;
                existing.Description = merchant.Description;
                existing.PayinFeePct = merchant.PayinFeePct;
                existing.PayoutFeePct = merchant.PayoutFeePct;
                existing.PayinMinAmount = merchant.PayinMinAmount;
                existing.PayinMaxAmount = merchant.PayinMaxAmount;
                existing.PayoutMinAmount = merchant.PayoutMinAmount;
                existing.PayoutMaxAmount = merchant.PayoutMaxAmount;
                existing.DiscardCent = merchant.DiscardCent;
                existing.IncludeGlobalBankAccount = merchant.IncludeGlobalBankAccount;
                existing.WhitelistBankAccountNames = merchant.WhitelistBankAccountNames;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MMerchant?> UpdateMerchantStatusById(string merchantId, string status)
        {
            Guid id = Guid.Parse(merchantId);
            var existing = await context!.Merchants!.AsExpandable().Where(p => p!.Id!.Equals(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = status;
                await context.SaveChangesAsync();
            }

            return existing;
        }
    }
}