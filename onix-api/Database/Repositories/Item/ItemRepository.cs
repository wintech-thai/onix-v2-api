using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class ItemRepository : BaseRepository, IItemRepository
    {
        public ItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MItem AddItem(MItem item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            context!.Items!.Add(item);
            context.SaveChanges();

            return item;
        }

        private ExpressionStarter<MItem> ItemPredicate(VMItem param)
        {
            var pd = PredicateBuilder.New<MItem>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MItem>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.ItemType != null) && (param.ItemType > 0))
            {
                var itemPd = PredicateBuilder.New<MItem>();
                itemPd = itemPd.Or(p => p.ItemType!.Equals(param.ItemType));

                pd = pd.And(itemPd);
            }

            return pd;
        }

        public int GetItemCount(VMItem param)
        {
            var predicate = ItemPredicate(param);
            var cnt = context!.Items!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MItem> GetItems(VMItem param)
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

            var predicate = ItemPredicate(param!);
            var arr = context!.Items!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MItem GetItemById(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = context!.Items!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MItem GetItemByName(string code)
        {
            var u = context!.Items!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsItemCodeExist(string code)
        {
            var cnt = context!.Items!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MItem? DeleteItemById(string ItemId)
        {
            Guid id = Guid.Parse(ItemId);

            var r = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.Items!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MItem? UpdateItemById(string itemId, MItem item)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Description = item.Description;
                result.Narrative = item.Narrative;
                result.Tags = item.Tags;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}