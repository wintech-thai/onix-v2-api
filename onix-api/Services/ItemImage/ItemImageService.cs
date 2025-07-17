using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ItemImageService : BaseService, IItemImageService
    {
        private readonly IItemImageRepository? repository = null;

        public ItemImageService(IItemImageRepository repo) : base()
        {
            repository = repo;
        }

        public MItemImage GetItemImageById(string orgId, string itemImageId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemImageById(itemImageId);

            return result;
        }

        public MVItemImage? AddItemImage(string orgId, MItemImage itemImage)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVItemImage();
            var result = repository!.AddItemImage(itemImage);

            r.Status = "OK";
            r.Description = "Success";
            r.ItemImage = result;

            return r;
        }

        public MVItemImage? UpdateItemImageById(string orgId, string itemImageId, MItemImage itemImage)
        {
            var r = new MVItemImage()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateItemImageById(itemImageId, itemImage);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item image ID [{itemImageId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ItemImage = result;
            return r;
        }

        public MVItemImage? DeleteItemImageByItemId(string orgId, string itemId)
        {
            var r = new MVItemImage()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(itemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Item ID [{itemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteItemImageByItemId(itemId);

            r.ItemImages = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public MVItemImage? DeleteItemImageById(string orgId, string itemImageId)
        {
            var r = new MVItemImage()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(itemImageId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Item image ID [{itemImageId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteItemImageById(itemImageId);

            r.ItemImage = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item image ID [{itemImageId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MItemImage> GetItemImages(string orgId, VMItemImage param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemImages(param);

            return result;
        }

        public int GetItemImageCount(string orgId, VMItemImage param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemImageCount(param);

            return result;
        }
    }
}
