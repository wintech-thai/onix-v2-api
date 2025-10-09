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
        private readonly IStorageUtils _storageUtil;

        public ItemImageService(IItemImageRepository repo, IStorageUtils storageUtil) : base()
        {
            repository = repo;
            _storageUtil = storageUtil;
        }

        public MVPresignedUrl GetItemImageUploadPresignedUrl(string orgId, string itemId)
        {
            var type = "png";
            var sec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
            var objectName = $"{Environment.GetEnvironmentVariable("ENV_GROUP")}/{orgId}/Products/{itemId}.{sec}.{type}";
            var validFor = TimeSpan.FromMinutes(15);
            var contentType = $"image/{type}";

            var url = _storageUtil.GenerateUploadUrl(bucket, objectName, validFor, contentType);
            //var previewUrl = _storageUtil.GenerateDownloadUrl(objectName, validFor, contentType);

            var result = new MVPresignedUrl()
            {
                Status = "SUCCESS",
                Description = "",
                PresignedUrl = url,
                ObjectName = objectName,
                ImagePath = objectName,
                //PreviewUrl = previewUrl,
            };

            return result;
        }

        public MItemImage GetItemImageById(string orgId, string itemImageId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemImageById(itemImageId);

            return result;
        }

        public MVItemImage? AddItemImage(string orgId, MItemImage itemImage)
        {
            var r = new MVItemImage();
            r.Status = "OK";
            r.Description = "Success";

            repository!.SetCustomOrgId(orgId);

            if (!string.IsNullOrEmpty(itemImage.ImagePath))
            {
                if (!_storageUtil.IsObjectExist(itemImage.ImagePath))
                {
                    r.Status = "OBJECT_NOT_FOUND";
                    r.Description = $"Object name [{itemImage.ImagePath}] not found!!!";
                    return r;
                }

                //Update metadata onix-is-temp-file to 'false'
                var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
                _storageUtil.UpdateMetaData(bucket, itemImage.ImagePath, "onix-is-temp-file", "false");
                
                //TODO : Allow only image .png to be uploaded
            }

            var result = repository!.AddItemImage(itemImage);
            r.ItemImage = result;

            return r;
        }

        public MVItemImage? UpdateItemImageById(string orgId, string itemImageId, MItemImage itemImage)
        {
            //เพื่อความสะดวก จะไม่ให้มีการอัพเดตรูป แต่ให้อัพเดตแค่เฉพาะ metadata ของรูป
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
            var images = repository!.DeleteItemImageByItemId(itemId);

            r.ItemImages = images;
            if (images == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";
                return r;
            }

            // images จะมีค่าเดิมก่อนที่จะถูก deleted ใน DB 
            foreach (var image in images)
            {
                DeleteStorageObject(image);
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
                return r;
            }

            // m จะมีค่าเป็น object เดิมก่อน delete อยู่แล้ว
            DeleteStorageObject(m);

            return r;
        }

        private void DeleteStorageObject(MItemImage m)
        {
            if (m == null)
            {
                return;
            }

            var objectName = m.ImagePath;
            if (string.IsNullOrEmpty(objectName))
            {
                return;
            }

            var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
            _storageUtil.DeleteObject(bucket, objectName);
        }

        public IEnumerable<MItemImage> GetItemImages(string orgId, VMItemImage param)
        {
            repository!.SetCustomOrgId(orgId);
            var images = repository!.GetItemImages(param);

            var validFor = TimeSpan.FromMinutes(60);
            var contentType = "image/png";
            foreach (var img in images)
            {
                if (!string.IsNullOrEmpty(img.ImagePath))
                {
                    img.ImageUrl = _storageUtil.GenerateDownloadUrl(img.ImagePath!, validFor, contentType);
                }
            }

            return images;
        }
        
        public IEnumerable<MItemImage> GetItemImagesByItemId(string orgId, string itemId)
        {
            var param = new VMItemImage() { ItemId = itemId } ;
            var result = GetItemImages(orgId, param);

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
