using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using Microsoft.Extensions.Options;

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
            var previewUrl = _storageUtil.GenerateDownloadUrl(objectName, validFor, contentType);

            var result = new MVPresignedUrl()
            {
                Status = "SUCCESS",
                Description = "",
                PresignedUrl = url,
                ObjectName = objectName,
                ImagePath = objectName,
                PreviewUrl = previewUrl,
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

                var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;

                //Allow only PNG to be uploaded
                var validateResult = ValidateImageFormat(bucket, itemImage.ImagePath);
                if (validateResult.Status != "OK")
                {
                    //ให้ลบไฟล์ที่ upload มาออกไปเลย ไม่เก็บไว้ให้เป็นภาระ
                    DeleteStorageObject(itemImage);

                    r.Status = validateResult.Status;
                    r.Description = validateResult.Description;
                    return r;
                }

                //Update metadata onix-is-temp-file to 'false'
                _storageUtil.UpdateMetaData(bucket, itemImage.ImagePath, "onix-is-temp-file", "false");
            }

            var result = repository!.AddItemImage(itemImage);
            r.ItemImage = result;

            return r;
        }

        private ValidationResult ValidateImageFormat(string bucket, string objectName)
        {
            ulong maxFileSize = 1 * 1024 * 1024; // 1 MB
            int maxWidth = 1200;
            int maxHeight = 1200;

            //วิธีนี้ใช้ได้แต่เฉพาะไฟล์ PNG เท่านั้น
            var r = new ValidationResult() { Status = "OK", Description = "" };

            var obj = _storageUtil.GetStorageObject(bucket, objectName);
            if (obj == null)
            {
                r.Status = "OBJECT_NOT_FOUND";
                r.Description = $"Object name [{objectName}] not found !!!";
                return r;
            }

            if (obj.Size > maxFileSize)
            {
                r.Status = "FILE_TOO_BIG";
                r.Description = $"File must be less than 1MB !!!";
                return r;
            }

            if (obj.ContentType != "image/png")
            {
                r.Status = "FILE_TYPE_NOT_PNG";
                r.Description = $"Content type must be 'image/png' !!!";
                return r;
            }
        
            var t = _storageUtil.PartialDownloadToStream(bucket, objectName, 0, 24);
            var header = t.Result;

            if (header.Length < 24)
            {
                r.Status = "NOT_VALID_PNG_FILE";
                r.Description = "File is not a valid PNG image!!!";
                return r;
            }
    
            int width = ServiceUtils.ReadInt32BigEndian(header, 16);
            int height = ServiceUtils.ReadInt32BigEndian(header, 20);

            if (width > maxWidth || height > maxHeight)
            {
                r.Status = "INVALID_IMAGE_DIMENSION";
                r.Description = $"Image dimention [w={width},h={height}] must be less than [w={maxWidth},h={maxHeight}]";
                return r;
            }
        
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
