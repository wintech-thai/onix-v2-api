using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class InventoryItemRepository : BaseRepository, IInventoryItemRepository
    {
        public InventoryItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MInventoryItem AddInventoryItem(MInventoryItem inventoryItem)
        {
            inventoryItem.Id = Guid.NewGuid();
            inventoryItem.CreatedDate = DateTime.UtcNow;
            inventoryItem.UpdatedDate = DateTime.UtcNow;
            inventoryItem.OrgId = orgId;

            context!.InventoryItems!.Add(inventoryItem);
            context.SaveChanges();

            return inventoryItem;
        }

        private ExpressionStarter<MInventoryItem> InventoryItemPredicate(VMInventoryItem param)
        {
            var pd = PredicateBuilder.New<MInventoryItem>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MInventoryItem>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.NameTh!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.NameEn!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.ItemType != null) && (param.ItemType != ""))
            {
                var typePd = PredicateBuilder.New<MInventoryItem>();
                typePd = typePd.Or(p => p.ItemType!.Equals(param.ItemType));

                pd = pd.And(typePd);
            }

            return pd;
        }

        public int GetInventoryItemCount(VMInventoryItem param)
        {
            var predicate = InventoryItemPredicate(param);
            var cnt = context!.InventoryItems!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MInventoryItem> GetInventoryItems(VMInventoryItem param)
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

            var predicate = InventoryItemPredicate(param!);
            var arr = context!.InventoryItems!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MInventoryItem GetInventoryItemById(string inventoryItemId)
        {
            Guid id = Guid.Parse(inventoryItemId);

            var u = context!.InventoryItems!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsInventoryItemCodeExist(string code)
        {
            var cnt = context!.InventoryItems!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MInventoryItem? DeleteInventoryItemById(string inventoryItemId)
        {
            Guid id = Guid.Parse(inventoryItemId);

            var r = context!.InventoryItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.InventoryItems!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MInventoryItem? UpdateInventoryItemById(string inventoryItemId, MInventoryItem inventoryItem)
        {
            Guid id = Guid.Parse(inventoryItemId);
            var result = context!.InventoryItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                //Not allow to update code
                result.ReferenceCode = inventoryItem.ReferenceCode;
                result.NameTh = inventoryItem.NameTh;
                result.NameEn = inventoryItem.NameEn;
                result.ItemType = inventoryItem.ItemType;
                result.Unit = inventoryItem.Unit;
                result.ItemGroup = inventoryItem.ItemGroup;
                result.Remark = inventoryItem.Remark;
                result.MinimumQuantity = inventoryItem.MinimumQuantity;
                result.Price = inventoryItem.Price;
                result.IsVatIncluded = inventoryItem.IsVatIncluded;

                if (!string.IsNullOrEmpty(inventoryItem.ImageBase64))
                {
                    result.ImageBase64 = inventoryItem.ImageBase64;
                }

                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}
