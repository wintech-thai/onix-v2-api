using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemRepository : BaseRepository, IScanItemRepository
    {
        public ScanItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

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
                context!.SaveChanges();
            }

            return result!;
        }

        public MScanItem AttachScanItemToCustomer(string itemId, string customerId, MEntity customer)
        {
            //TODO : Use customer in the future to get customer code
            Guid cid = Guid.Parse(customerId);
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.AppliedFlag = "TRUE";
                result.CustomerId = cid;
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

                pd = pd.And(fullTextPd);
            }

            return pd;
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