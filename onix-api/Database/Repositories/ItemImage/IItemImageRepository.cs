using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IItemImageRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MItemImage AddItemImage(MItemImage itemImageId);
        public int GetItemImageCount(VMItemImage param);
        public IEnumerable<MItemImage> GetItemImages(VMItemImage param);
        public MItemImage GetItemImageById(string itemImageId);
        public MItemImage? DeleteItemImageById(string itemImageId);
        public List<MItemImage>? DeleteItemImageByItemId(string itemId);
        public MItemImage? UpdateItemImageById(string itemImageId, MItemImage itemImage);
    }
}
