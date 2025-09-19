using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;

namespace Its.Onix.Api.Services
{
    public class ScanItemService : BaseService, IScanItemService
    {
        private readonly IScanItemRepository? repository = null;
        private readonly IItemRepository _itemRepo;
        private readonly IItemImageRepository _imageItemRepo;
        private readonly IStorageUtils _storageUtil;
        private readonly RedisHelper _redis;

        public ScanItemService(IScanItemRepository repo,
            IItemRepository itemRepo,
            IItemImageRepository imageItemRepo,
            IStorageUtils storageUtil,
            RedisHelper redis) : base()
        {
            repository = repo;
            _itemRepo = itemRepo;
            _imageItemRepo = imageItemRepo;
            _storageUtil = storageUtil;
            _redis = redis;
        }

        public MVScanItem AttachScanItemToProduct(string orgId, string itemId, string productId)
        {
            var r = new MVScanItem()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.AttachScanItemToProduct(itemId, productId);

            r.ScanItem = result;
            
            return r;
        }

        public MVItem GetScanItemProduct(string orgId, string serial, string pin, string otp)
        {
            _itemRepo!.SetCustomOrgId(orgId);
            _imageItemRepo!.SetCustomOrgId(orgId);
            repository!.SetCustomOrgId(orgId);

            var r = new MVItem()
            {
                Status = "SUCCESS",
                Description = "",
            };

            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "GetProduct");
            var otpObj = _redis.GetObjectAsync<MOtp>(cacheKey).Result;
            if (otpObj == null)
            {
                r.Status = "OTP_NOTFOUND_OR_EXPIRE";
                r.Description = $"OTP [{otp}] not found or expire!!!";

                return r;
            }

            if (otpObj.Otp != otp)
            {
                r.Status = "OTP_INVALID";
                r.Description = $"OTP [{otp}] is invalid (not match)!!!";

                return r;
            }

            var result = repository!.GetScanItemBySerialPin(serial, pin);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";

                return r;
            }

            var productId = result.ItemId.ToString();
            if (productId == null)
            {
                r.Status = "PRODUCT_NOT_ATTACH";
                r.Description = $"No product attached to this scan item!!!";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(productId))
            {
                r.Status = "PRODUCT_ID_INVALID";
                r.Description = $"Product ID [{productId}] invalid!!!";

                return r;
            }

            var product = _itemRepo.GetItemById(productId!);

            if (product == null)
            {
                r.Status = "PRODUCT_NOTFOUND";
                r.Description = $"Product ID [{productId}] not found!!!";

                return r;
            }

            var imageParam = new VMItemImage()
            {
                ItemId = productId,
            };

            var images = _imageItemRepo.GetImages(imageParam);
            product.Images = []; //This is important to set this otherwise we will see error 502 for some reason
            product.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(product.Properties!);
            product.Properties = "";

            r.Item = product;

            var validFor = TimeSpan.FromMinutes(60);
            var contentType = "image/png";
            var imageList = images.ToList(); 

            foreach (var img in imageList)
            {
                if (!string.IsNullOrEmpty(img.ImagePath))
                {
                    img.ImageUrl = _storageUtil.GenerateDownloadUrl(img.ImagePath!, validFor, contentType);
                }
            }

            r.Images = imageList;
            return r;
        }

        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin)
        {
            var r = new MVScanItemResult()
            {
                Status = "SUCCESS",
                DescriptionEng = $"Your product serial=[{serial}] และ pin=[{pin}] is genuine.",
                DescriptionThai = $"สินค้า ซีเรียล=[{serial}] และ พิน=[{pin}] เป็นของแท้",
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemBySerialPin(serial, pin);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.DescriptionEng = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";
                r.DescriptionThai = $"ไม่พบ ซีเรียล=[{serial}] และ พิน=[{pin}] ในฐานข้อมูล!!!";

                return r;
            }

            r.ScanItem = result;
            var id = result.Id.ToString();

            if (result.RegisteredFlag!.Equals("TRUE"))
            {
                r.Status = "ALREADY_REGISTERED";
                r.DescriptionEng = $"Your product serial=[{serial}] and pin=[{pin}] is already registered!!!";
                r.DescriptionThai = $"สินค้า ซีเรียล=[{serial}] และ พิน=[{pin}] เคยลงทะเบียนแล้ว!!!";

                repository.IncreaseScanCount(id!);

                return r;
            }

            r.ScanItem = repository.RegisterScanItem(id!);
            return r;
        }
    }
}
