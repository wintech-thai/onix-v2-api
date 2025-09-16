using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IItemImageService
    {
        public MItemImage GetItemImageById(string orgId, string itemImageId);
        public MVItemImage? AddItemImage(string orgId, MItemImage itemImage);
        public MVItemImage? DeleteItemImageById(string orgId, string itemImageId);
        public MVItemImage? DeleteItemImageByItemId(string orgId, string itemId);
        public IEnumerable<MItemImage> GetItemImages(string orgId, VMItemImage param);
        public int GetItemImageCount(string orgId, VMItemImage param);
        public MVItemImage? UpdateItemImageById(string orgId, string itemImageId, MItemImage itemImage);
        public string GetItemImageUploadPresignedUrl(string orgId, string itemId);
    }
}
