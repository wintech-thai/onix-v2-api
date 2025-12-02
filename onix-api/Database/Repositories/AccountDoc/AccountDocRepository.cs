using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class AccountDocRepository : BaseRepository, IAccountDocRepository
    {
        public AccountDocRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsAccountDocNoExist(string docNo)
        {
            var c = await context!.AccountDocs!.Where(p => p!.Code!.Equals(docNo) && p!.OrgId!.Equals(orgId)).CountAsync();
            return c == 0;    
        }

        public async Task<bool> IsAccountDocApproved(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);
            var c = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId) && p!.Status!.Equals("Approved")).CountAsync();
            return c > 0;
        }

        public async Task<bool> IsAccountDocItemApproved(string accountDocItemId)
        {
            var accDocItemId = Guid.Parse(accountDocItemId);

            var approved = await (
                from ai in context!.AccountDocItems
                join ad in context.AccountDocs!
                    on ai.AccountDocId equals ad.Id.ToString() into items
                from item in items.DefaultIfEmpty()
                where ai.Id == accDocItemId
                    && ai.OrgId == orgId
                    && item != null
                    && item.Status == "Approved"
                select ai.Id
            ).AnyAsync();

            return approved;
        }

        public async Task<MAccountDocItem?> UpdateAccountDocItemById(string accountDocItemId, MAccountDocItem adi)
        {
            Guid id = Guid.Parse(accountDocItemId);
            var existing = await context!.AccountDocItems!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Quantity = adi.Quantity;
                existing.UnitPrice = adi.UnitPrice;
                existing.TotalPrice = adi.TotalPrice;
                existing.Tags = adi.Tags;
            }

            await context.SaveChangesAsync();

            //TODO : Added update total price to parent document

            return existing;
        }

        public async Task<MAccountDocItem?> DeleteAccountDocItemById(string accountDocItemId)
        {
            Guid id = Guid.Parse(accountDocItemId);
            var existing = await context!.AccountDocItems!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.AccountDocItems!.Remove(existing);
                await context.SaveChangesAsync();

                //TODO : Added update total price to parent document
            }

            return existing;
        }

        public async Task<MAccountDocItem> AddAccountDocItem(string accountDocId, MAccountDocItem adi)
        {
            adi.OrgId = orgId;
            adi.AccountDocId = accountDocId;

            await context!.AccountDocItems!.AddAsync(adi);
            await context.SaveChangesAsync();

            //TODO : Added update total price to parent document

            return adi;
        }

        public async Task<List<MAccountDocItem>> GetAccountDocItemsById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);

            var arr = await (from ai in context!.AccountDocItems
                       join it in context.Items!
                       on ai.ProductId equals it.Id.ToString() into items
                       from item in items.DefaultIfEmpty()
                       select new MAccountDocItem
                       {
                            Id = ai.Id,
                            OrgId = ai.OrgId,
                            AccountDocId = ai.AccountDocId,
                            ProductId = ai.ProductId,
                            IncentiveRate = ai.IncentiveRate,
                            IncentiveTotalPrice = ai.IncentiveTotalPrice,
                            Tags = ai.Tags,
                            Quantity = ai.Quantity,
                            UnitPrice = ai.UnitPrice,
                            TotalPrice = ai.TotalPrice,
                            ProductCode = item != null ? item.Code : "",
                            ProductDesc = item != null ? item.Description : "",
                       })
                .Where(ai => ai.AccountDocId!.Equals(id) && ai.OrgId!.Equals(orgId))
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return arr;
        }

        public async Task<MAccountDoc?> UpdateAccountDocById(string accountDocId, MAccountDoc ad)
        {
            Guid id = Guid.Parse(accountDocId);
            var existing = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.DocumentDate = ad.DocumentDate;
                existing.Code = ad.Code;
                existing.Description = ad.Description;
                existing.EntityId = ad.EntityId;
                existing.Tags = ad.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MAccountDoc?> ApproveAccountDocById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);
            var existing = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = "Approved";
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MAccountDoc?> DeleteAccountDocById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);

            var docItems = await context!.AccountDocItems!.Where(p => p!.AccountDocId!.Equals(accountDocId) && p!.OrgId!.Equals(orgId)).ToListAsync();
            if (docItems.Count > 0)
            {
                context.AccountDocItems!.RemoveRange(docItems);
                await context.SaveChangesAsync();
            }
            
            var existing = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.AccountDocs!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MAccountDoc> AddAccountDoc(MAccountDoc ad)
        {
            ad.OrgId = orgId;

            await context!.AccountDocs!.AddAsync(ad);
            await context.SaveChangesAsync();

            return ad;
        }

        private ExpressionStarter<MAccountDoc> AccountDocPredicate(VMAccountDoc param)
        {
            var pd = PredicateBuilder.New<MAccountDoc>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MAccountDoc>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Status!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<List<MAccountDoc>> GetAccountDocs(VMAccountDoc param)
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

            var predicate = AccountDocPredicate(param!);
            var result = await context!.AccountDocs!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            //มันใหญ่มากเลยไม่อยากให้ return params ออกไป
            foreach (var r in result)
            {
                r.DocumentParams = "";
            }

            return result;
        }

        public async Task<int> GetAccountDocCount(VMAccountDoc param)
        {
            var predicate = AccountDocPredicate(param!);
            var result = await context!.AccountDocs!.Where(predicate).CountAsync();

            return result;
        }

        public async Task<MAccountDoc?> GetAccountDocById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);
            var u = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }
    }
}
