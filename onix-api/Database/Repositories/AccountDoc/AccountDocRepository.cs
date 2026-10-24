using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

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

        private async Task<MAccountDoc> UpdateAccountDocTotal(string accountDocId)
        {
            var items = await context!.AccountDocItems!
                .Where(x => x.AccountDocId == accountDocId)
                .ToListAsync();

            // 2. คำนวณราคารวมใหม่
            var newTotal = items.Sum(x => x.TotalPrice);

            // 3. อัปเดตลง parent document
            var parentDoc = await context.AccountDocs!
                .FirstOrDefaultAsync(x => x.Id.ToString() == accountDocId);

            if (parentDoc != null)
            {
                parentDoc.TotalPrice = newTotal; 
                context.AccountDocs!.Update(parentDoc);
            }

            return parentDoc!;
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

                await UpdateAccountDocTotal(existing.AccountDocId!);
            }

            await context.SaveChangesAsync();

            return existing;
        }

        public async Task<MAccountDocItem?> DeleteAccountDocItemById(string accountDocItemId)
        {
            Guid id = Guid.Parse(accountDocItemId);
            var existing = await context!.AccountDocItems!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.AccountDocItems!.Remove(existing);
                await UpdateAccountDocTotal(existing.AccountDocId!);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MAccountDocItem> AddAccountDocItem(string accountDocId, MAccountDocItem adi)
        {
            adi.OrgId = orgId;
            adi.AccountDocId = accountDocId;

            await context!.AccountDocItems!.AddAsync(adi);
            await UpdateAccountDocTotal(accountDocId);
            await context.SaveChangesAsync();

            return adi;
        }

        public async Task<List<MAccountDocItem>> GetAccountDocItemsById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);

            // ProductCode/ProductDesc ถูก denormalize เก็บไว้ตอนบันทึกแล้ว (ดู SyncAccountDocItems)
            // ไม่ต้อง join สด เพราะแถวแบบ Custom ไม่มี master record ให้ join
            var arr = await context!.AccountDocItems!
                .Where(ai => ai.AccountDocId!.Equals(id.ToString()) && ai.OrgId!.Equals(orgId))
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return arr;
        }

        // x011 ปรับแก้ #7: item discount (%/จำนวนเงิน) + VAT + หัก ณ ที่จ่าย ต่อ item
        // คำนวณจาก NetAmount = TotalPrice(gross) - ItemDiscountAmount, ใช้ header VatRate เดียวกันทุก item
        private static void CalculateItemAmounts(MAccountDocItem item, double vatRate)
        {
            var gross = (item.Quantity ?? 0) * (item.UnitPrice ?? 0);
            item.TotalPrice = gross;

            var discountAmount = item.ItemDiscountType switch
            {
                "Percent" => gross * ((item.ItemDiscountValue ?? 0) / 100.0),
                "Amount" => item.ItemDiscountValue ?? 0,
                _ => 0,
            };
            item.ItemDiscountAmount = discountAmount;

            var netAmount = gross - discountAmount;
            item.NetAmount = netAmount;

            item.VatAmount = (item.VatEnabled == true) ? netAmount * (vatRate / 100.0) : 0;
            item.WithholdingTaxAmount = (item.WithholdingTaxEnabled == true) ? netAmount * ((item.WithholdingTaxRate ?? 0) / 100.0) : 0;
        }

        // Reused by Add (existing items always empty) and Update (real reconciliation) — same
        // diff-sync idea as InventoryDocRepository.SyncInventoryDocItems.
        private async Task SyncAccountDocItems(Guid accountDocId, List<MAccountDocItem>? incomingItems, double vatRate)
        {
            var incoming = incomingItems ?? new List<MAccountDocItem>();
            var existingItems = await context!.AccountDocItems!
                .Where(i => i.AccountDocId!.Equals(accountDocId.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            var incomingIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

            foreach (var existing in existingItems)
            {
                if (!existing.Id.HasValue || !incomingIds.Contains(existing.Id.Value))
                {
                    context!.AccountDocItems!.Remove(existing);
                }
            }

            foreach (var item in incoming)
            {
                CalculateItemAmounts(item, vatRate);

                var existing = item.Id.HasValue
                    ? existingItems.FirstOrDefault(e => e.Id == item.Id)
                    : null;

                if (existing != null)
                {
                    existing.ProductId = item.ProductId;
                    existing.ProductType = item.ProductType;
                    existing.ProductCode = item.ProductCode;
                    existing.ProductDesc = item.ProductDesc;
                    existing.Quantity = item.Quantity;
                    existing.UnitPrice = item.UnitPrice;
                    existing.TotalPrice = item.TotalPrice;
                    existing.Tags = item.Tags;
                    existing.ItemDiscountType = item.ItemDiscountType;
                    existing.ItemDiscountValue = item.ItemDiscountValue;
                    existing.ItemDiscountAmount = item.ItemDiscountAmount;
                    existing.NetAmount = item.NetAmount;
                    existing.VatEnabled = item.VatEnabled;
                    existing.VatAmount = item.VatAmount;
                    existing.WithholdingTaxEnabled = item.WithholdingTaxEnabled;
                    existing.WithholdingTaxRate = item.WithholdingTaxRate;
                    existing.WithholdingTaxAmount = item.WithholdingTaxAmount;
                    existing.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    item.Id = Guid.NewGuid();
                    item.OrgId = orgId;
                    item.AccountDocId = accountDocId.ToString();
                    item.CreatedDate = DateTime.UtcNow;
                    item.UpdatedDate = DateTime.UtcNow;
                    await context!.AccountDocItems!.AddAsync(item);
                }
            }

            await context!.SaveChangesAsync();

            var allItems = await context!.AccountDocItems!
                .Where(i => i.AccountDocId!.Equals(accountDocId.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            var doc = await context!.AccountDocs!.FirstOrDefaultAsync(d => d.Id!.Equals(accountDocId) && d.OrgId!.Equals(orgId));
            if (doc != null)
            {
                doc.VatRate = vatRate;
                doc.TotalPrice = allItems.Sum(i => i.TotalPrice ?? 0);
                doc.DiscountAmount = allItems.Sum(i => i.ItemDiscountAmount ?? 0);
                doc.NetAmount = doc.TotalPrice - doc.DiscountAmount;
                doc.VatAmount = allItems.Sum(i => i.VatAmount ?? 0);
                doc.TotalWithVat = doc.NetAmount + doc.VatAmount;
                doc.WithholdingTaxAmount = allItems.Sum(i => i.WithholdingTaxAmount ?? 0);
                doc.GrandTotal = doc.TotalWithVat - doc.WithholdingTaxAmount;
                await context!.SaveChangesAsync();
            }
        }

        // Callers (the service layer) must check IsAccountDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MAccountDoc?> UpdateAccountDocById(string accountDocId, MAccountDoc ad)
        {
            Guid id = Guid.Parse(accountDocId);
            var existing = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing == null) return null;

            existing.DocumentDate = ad.DocumentDate;
            existing.DueDate = ad.DueDate;
            existing.Description = ad.Description;
            existing.EntityId = ad.EntityId;
            existing.Tags = ad.Tags;
            existing.PaymentMethod = ad.PaymentMethod;
            existing.PaidAmount = ad.PaidAmount;
            existing.UpdatedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            await SyncAccountDocItems(existing.Id!.Value, ad.Items, ad.VatRate ?? 0);

            existing.Items = await context!.AccountDocItems!
                .Where(i => i.AccountDocId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            return existing;
        }

        // Callers (the service layer) must check IsAccountDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MAccountDoc?> ApproveAccountDocById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);
            var existing = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing == null) return null;

            var now = DateTime.UtcNow;
            existing.Status = "Approved";
            existing.ApprovedDate = now;
            existing.StatusDate = now;
            await context.SaveChangesAsync();

            return existing;
        }

        // Callers (the service layer) must check IsAccountDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MAccountDoc?> CancelAccountDocById(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);
            var existing = await context!.AccountDocs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing == null) return null;

            existing.Status = "Cancelled";
            existing.StatusDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> IsAccountDocPending(string accountDocId)
        {
            Guid id = Guid.Parse(accountDocId);
            var doc = await context!.AccountDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            return doc != null && doc.Status == "Pending";
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
            var items = ad.Items;
            ad.Items = null;

            ad.Id = Guid.NewGuid();
            ad.OrgId = orgId;
            ad.Status = "Pending";
            ad.CreatedDate = DateTime.UtcNow;
            ad.UpdatedDate = DateTime.UtcNow;
            ad.ApprovedDate = null;
            ad.StatusDate = null;

            await context!.AccountDocs!.AddAsync(ad);
            await context.SaveChangesAsync();

            await SyncAccountDocItems(ad.Id!.Value, items, ad.VatRate ?? 0);

            ad.Items = await context!.AccountDocItems!
                .Where(i => i.AccountDocId!.Equals(ad.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

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

            if ((param.DocumentType != "") && (param.DocumentType != null))
            {
                var docTypePd = PredicateBuilder.New<MAccountDoc>();
                docTypePd = docTypePd.Or(p => p.DocumentType!.Equals(param.DocumentType));

                pd = pd.And(docTypePd);
            }

            if (!string.IsNullOrEmpty(param.DocumentStatus))
            {
                pd = pd.And(p => p.Status!.Equals(param.DocumentStatus));
            }

            if (!string.IsNullOrEmpty(param.EntityId))
            {
                pd = pd.And(p => p.EntityId!.Equals(param.EntityId));
            }

            if (param.FromDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate >= param.FromDate.Value);
            }

            if (param.ToDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate <= param.ToDate.Value);
            }

            return pd;
        }

        private async Task PopulateEntityInfo(IEnumerable<MAccountDoc> docs)
        {
            var list = docs.ToList();
            var entityIds = list.Where(d => !string.IsNullOrEmpty(d.EntityId)).Select(d => Guid.Parse(d.EntityId!)).Distinct().ToList();
            if (entityIds.Count == 0) return;

            var entities = await context!.Entities!
                .Where(e => e.OrgId!.Equals(orgId) && entityIds.Contains(e.Id!.Value))
                .ToDictionaryAsync(e => e.Id!.Value, e => e);

            foreach (var d in list)
            {
                if (string.IsNullOrEmpty(d.EntityId)) continue;
                if (entities.TryGetValue(Guid.Parse(d.EntityId), out var entity))
                {
                    d.EntityCode = entity.Code;
                    d.EntityName = entity.Name;
                }
            }
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

            await PopulateEntityInfo(result);

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
            if (u == null) return null;

            u.Items = await context!.AccountDocItems!
                .Where(i => i.AccountDocId!.Equals(u.Id.ToString()) && i.OrgId!.Equals(orgId))
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            await PopulateEntityInfo([u]);

            return u;
        }
    }
}
