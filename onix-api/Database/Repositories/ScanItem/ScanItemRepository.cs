using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.RegularExpressions;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemRepository : BaseRepository, IScanItemRepository
    {
        public ScanItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

        //=== Start V2 ===
        public IQueryable<MScanItem> GetSelectionV2()
        {
            var query =
                from sci in context!.ScanItems

                join cst in context.Entities!
                    on sci.CustomerId equals cst.Id into customers
                from customer in customers.DefaultIfEmpty()

                join itm in context.Items!
                    on sci.ItemId equals itm.Id into items
                from item in items.DefaultIfEmpty()

                join scf in context.ScanItemFolders!
                    on sci.FolderId equals scf.Id into folders
                from folder in folders.DefaultIfEmpty()

                join sca in context.ScanItemActions!
                    on folder.ScanItemActionId equals sca.Id.ToString() into actions
                from action in actions.DefaultIfEmpty()

                join prd in context.Items!
                    on folder.ProductId equals prd.Id.ToString() into products
                from product in products.DefaultIfEmpty()

                select new { sci, customer, folder, action, product, item };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MScanItem
            {
                Id = x.sci.Id,
                OrgId = x.sci.OrgId,
                Serial = x.sci.Serial,
                Pin = x.sci.Pin,
                Tags = x.sci.Tags,
                SequenceNo = x.sci.SequenceNo,
                Url = x.sci.Url,
                RunId = x.sci.RunId,
                UploadedPath = x.sci.UploadedPath,
                ItemGroup = x.sci.ItemGroup,
                RegisteredFlag = x.sci.RegisteredFlag,
                ScanCount = x.sci.ScanCount,
                UsedFlag = x.sci.UsedFlag,
                ItemId = x.sci.ItemId,
                AppliedFlag = x.sci.AppliedFlag,
                CustomerId = x.sci.CustomerId,
                FolderId = x.sci.FolderId,
                CreatedDate = x.sci.CreatedDate,
                RegisteredDate = x.sci.RegisteredDate,

                ScanItemActionId = x.action.Id.ToString(),
                ScanItemActionName = x.action.ActionName,
                ProductCode = x.product.Code,
                ProductDesc = x.product.Description,
                CustomerEmail = x.customer.PrimaryEmail,
                FolderName = x.folder.FolderName,

                ProductCodeLegacy = x.item.Code,
                ProductDescLegacy = x.item.Description,
            });
        }

        private ExpressionStarter<MScanItem> ScanItemPredicateV2(VMScanItem param)
        {
            var pd = PredicateBuilder.New<MScanItem>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));
            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MScanItem>();
                fullTextPd = fullTextPd.Or(p => p.Serial!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Pin!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ProductCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ProductCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ProductDesc!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ProductCodeLegacy!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ProductDescLegacy!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.FolderName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ScanItemActionName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.CustomerEmail!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<IEnumerable<MScanItem>> GetScanItemsV2(VMScanItem param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = ScanItemPredicateV2(param!);
            var arr = await GetSelectionV2().AsExpandable().Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }

        public async Task<int> GetScanItemCountV2(VMScanItem param)
        {
            var predicate = ScanItemPredicateV2(param);
            var cnt = await GetSelectionV2().AsExpandable().Where(predicate).CountAsync();

            return cnt;
        }

        public async Task<MScanItem> GetScanItemByIdV2(string scanItemId)
        {
            Guid id = Guid.Parse(scanItemId);
            var u = await GetSelectionV2().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();

            return u!;
        }

        //=== End V2 ===

        public MScanItem? GetScanItemBySerialPin(string serial, string pin)
        {
            var u = context!.ScanItems!.Where(p =>
                p!.Serial!.Equals(serial) &&
                p!.Pin!.Equals(pin) &&
                p!.OrgId!.Equals(orgId)).FirstOrDefault();

            return u!;
        }

        public MScanItem RegisterScanItem(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RegisteredFlag = "TRUE";
                result.RegisteredDate = DateTime.UtcNow;
                result.ScanCount = (result.ScanCount ?? 0) + 1;

                context!.SaveChanges();
            }

            return result!;
        }

        public MScanItem IncreaseScanCount(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {

                result.ScanCount = (result.ScanCount ?? 0) + 1;
                context!.SaveChanges();
            }

            return result!;
        }

        public MScanItem AttachScanItemToProduct(string itemId, string productId, MItem product)
        {
            Guid pid = Guid.Parse(productId);
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.UsedFlag = "TRUE";
                result.ItemId = pid;
                result.ProductCode = product.Code;
                result.Tags = UpdateTags(result.Tags!, "product", product.Code!);
                context!.SaveChanges();
            }

            return result!;
        }

        private string UpdateTags(string tags, string tagName, string tagValue)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return $"{tagName}={tagValue}";
            }
            
            // แยก string ด้วย comma
            var parts = tags.Split(',').ToList();

            // regex pattern สำหรับ tag
            var pattern = new Regex($"^{Regex.Escape(tagName)}=(.+)$");
            bool found = false;

            for (int i = 0; i < parts.Count; i++)
            {
                if (pattern.IsMatch(parts[i]))
                {
                    parts[i] = $"{tagName}={tagValue}";
                    found = true;
                    break;
                }
            }

            // ถ้าไม่พบ tag ใน list ให้ append ใหม่
            if (!found)
            {
                parts.Add($"{tagName}={tagValue}");
            }

            var result = string.Join(",", parts);
            return result;
        }

        public MScanItem AttachScanItemToCustomer(string itemId, string customerId, MEntity customer)
        {
            Guid cid = Guid.Parse(customerId);
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.AppliedFlag = "TRUE";
                result.CustomerId = cid;
                result.Tags = UpdateTags(result.Tags!, "email", customer.PrimaryEmail!);
                context!.SaveChanges();
            }

            return result!;
        }


        public MScanItem AddScanItem(MScanItem scanItem)
        {
            scanItem.Id = Guid.NewGuid();
            scanItem.CreatedDate = DateTime.UtcNow;
            scanItem.OrgId = orgId;

            context!.ScanItems!.Add(scanItem);
            context.SaveChanges();

            return scanItem;
        }

        private ExpressionStarter<MScanItem> ScanItemPredicate(VMScanItem param)
        {
            var pd = PredicateBuilder.New<MScanItem>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MScanItem>();
                fullTextPd = fullTextPd.Or(p => p.Serial!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Pin!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ProductCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<int> GetScanItemCountAsync(VMScanItem param)
        {
            var predicate = ScanItemPredicate(param);
            var cnt = await context!.ScanItems!.AsExpandable().Where(predicate).CountAsync();

            return cnt;
        }
        
        public async Task<IEnumerable<MScanItem>> GetScanItemsAsyn(VMScanItem param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = ScanItemPredicate(param!);
            var arr = await context!.ScanItems!.AsExpandable().Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }

        public int GetScanItemCount(VMScanItem param)
        {
            var predicate = ScanItemPredicate(param);
            var cnt = context!.ScanItems!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MScanItem> GetScanItems(VMScanItem param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = ScanItemPredicate(param!);
            var arr = context!.ScanItems!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MScanItem GetScanItemById(string scanItemId)
        {
            Guid id = Guid.Parse(scanItemId);
            var u = context!.ScanItems!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();

            return u!;
        }

        public MScanItem? DeleteScanItemById(string scanItemId)
        {
            Guid id = Guid.Parse(scanItemId);

            var r = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.ScanItems!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MScanItem? UnVerifyScanItemById(string scanItemId)
        {
            Guid id = Guid.Parse(scanItemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RegisteredFlag = "NO";
                context!.SaveChanges();
            }

            return result!;
        }

        public MScanItem DetachScanItemFromProduct(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.UsedFlag = "FALSE";
                result.ItemId = null;
                result.ProductCode = null;

                // Remove product tag from Tags
                if (!string.IsNullOrEmpty(result.Tags))
                {
                    var parts = result.Tags.Split(',').ToList();
                    parts = parts.Where(part => !part.StartsWith("product=")).ToList();
                    result.Tags = string.Join(",", parts);
                }

                context!.SaveChanges();
            }

            return result!;
        }

        public MScanItem DetachScanItemFromCustomer(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.AppliedFlag = "FALSE";
                result.CustomerId = null;

                // Remove email tag from Tags
                if (!string.IsNullOrEmpty(result.Tags))
                {
                    var parts = result.Tags.Split(',').ToList();
                    parts = parts.Where(part => !part.StartsWith("email=")).ToList();
                    result.Tags = string.Join(",", parts);
                }

                context!.SaveChanges();
            }

            return result!;
        }

        public bool IsSerialExist(string serial)
        {
            var cnt = context!.ScanItems!.Where(p => p!.Serial!.Equals(serial)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public bool IsPinExist(string pin)
        {
            var cnt = context!.ScanItems!.Where(p => p!.Pin!.Equals(pin)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }
    }
}