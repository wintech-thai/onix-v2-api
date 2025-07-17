using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class ItemImageRepository : BaseRepository, IItemImageRepository
    {
        public ItemImageRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MItemImage AddItemImage(MItemImage item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            context!.ItemImages!.Add(item);
            context.SaveChanges();

            return item;
        }

        private ExpressionStarter<MItemImage> ItemImagePredicate(VMItemImage param)
        {
            Guid id = Guid.Parse(param.ItemId!);
            var pd = PredicateBuilder.New<MItemImage>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            //Need to use ItemId as the query parameter
            var itemPd = PredicateBuilder.New<MItemImage>();
            itemPd = itemPd.Or(p => p.ItemId!.Equals(id));
            pd = pd.And(itemPd);

            if ((param.Category != null) && (param.Category > 0))
            {
                var imageItemPd = PredicateBuilder.New<MItemImage>();
                imageItemPd = imageItemPd.Or(p => p.Category!.Equals(param.Category));

                pd = pd.And(imageItemPd);
            }

            return pd;
        }

        public int GetItemImageCount(VMItemImage param)
        {
            var predicate = ItemImagePredicate(param);
            var cnt = context!.ItemImages!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MItemImage> GetItemImages(VMItemImage param)
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

            var predicate = ItemImagePredicate(param!);
            var arr = context!.ItemImages!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MItemImage GetItemImageById(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = context!.ItemImages!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MItemImage? DeleteItemImageById(string ItemImageId)
        {
            Guid id = Guid.Parse(ItemImageId);

            var r = context!.ItemImages!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.ItemImages!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public List<MItemImage>? DeleteItemImageByItemId(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var r = context!.ItemImages!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).ToList();
            if (r != null)
            {
                context!.ItemImages!.RemoveRange(r);
                context.SaveChanges();
            }

            return r;
        }

        public MItemImage? UpdateItemImageById(string itemId, MItemImage item)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.ItemImages!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Category = item.Category;
                result.Narative = item.Narative;
                result.Tags = item.Tags;
                result.ImagePath = item.ImagePath;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}