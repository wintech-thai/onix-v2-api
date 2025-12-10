using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Models;
using System.Text.Json;
using Serilog;

namespace Its.Onix.Api.Services
{
    public class ScanItemService : BaseService, IScanItemService
    {
        private readonly IScanItemRepository? repository = null;
        private readonly IItemRepository _itemRepo;
        private readonly IPointRepository _pointRepo;
        private readonly IItemImageRepository _imageItemRepo;
        private readonly IEntityRepository _entityRepo;
        private readonly IStorageUtils _storageUtil;
        private readonly IRedisHelper _redis;
        private readonly IJobService _jobService;
        private readonly IScanItemTemplateService _sciTemplateSvc;

        public ScanItemService(IScanItemRepository repo,
            IItemRepository itemRepo,
            IItemImageRepository imageItemRepo,
            IEntityRepository entityRepo,
            IPointRepository pointRepo,
            IStorageUtils storageUtil,
            IJobService jobService,
            IScanItemTemplateService sciTemplateService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _itemRepo = itemRepo;
            _imageItemRepo = imageItemRepo;
            _entityRepo = entityRepo;
            _storageUtil = storageUtil;
            _redis = redis;
            _jobService = jobService;
            _sciTemplateSvc = sciTemplateService;
            _pointRepo = pointRepo;
        }

        public MVScanItem AttachScanItemToProduct(string orgId, string scanItemId, string productId)
        {
            _itemRepo.SetCustomOrgId(orgId);
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItem()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(scanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Scan Item ID [{scanItemId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(productId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Product ID [{productId}] format is invalid";

                return r;
            }

            var product = _itemRepo.GetItemById(productId!);
            if (product == null)
            {
                r.Status = "PRODUCT_NOTFOUND";
                r.Description = $"Product ID [{productId}] not found!!!";

                return r;
            }
           
            var result = repository!.AttachScanItemToProduct(scanItemId, productId, product);
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
            var otpObj = _redis.GetObjectAsync<MOtp>($"{cacheKey}:{serial}:{pin}").Result;
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

            var result = repository!.GetScanItemBySerialPinV2(serial, pin);
            if (result.Result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";

                return r;
            }

            var productId = result.Result.ItemId.ToString();
            if (string.IsNullOrEmpty(productId))
            {
                r.Status = "PRODUCT_NOT_ATTACH";
                r.Description = $"No product attached to this scan item!!!";

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

        public MVEntity GetScanItemCustomer(string orgId, string serial, string pin, string otp)
        {
            _entityRepo!.SetCustomOrgId(orgId);
            repository!.SetCustomOrgId(orgId);

            var r = new MVEntity()
            {
                Status = "SUCCESS",
                Description = "",
            };

            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "GetCustomer");
            var otpObj = _redis.GetObjectAsync<MOtp>($"{cacheKey}:{serial}:{pin}").Result;
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

            var result = repository!.GetScanItemBySerialPinV2(serial, pin);
            if (result.Result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";

                return r;
            }

            var customerId = result.Result.CustomerId.ToString();
            if (string.IsNullOrEmpty(customerId))
            {
                r.Status = "CUSTOMER_NOT_ATTACH";
                r.Description = $"No customer attached to this scan item!!!";

                return r;
            }

            var customer = _entityRepo.GetEntityById(customerId!);

            if (customer == null)
            {
                //ถึงตรงนี้ให้เป็น SUCCESS เพราะว่า ถือว่ายังมี customer อยู่แต่ customer จริงๆโดนลบไปแล้วแต่ ID ยังค้างอยู่
                customer = new MEntity()
                {
                    Id = result.Result.CustomerId,
                    PrimaryEmail = ServiceUtils.GetValueFromTags("email", result.Result.Tags!),
                };

                if (string.IsNullOrEmpty(customer.PrimaryEmail))
                {
                    customer.PrimaryEmail = "customer-deleted@please-scan.com";
                }
            }

            r.Entity = customer;
            return r;
        }

        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin, bool isDryRun)
        {
            var r = new MVScanItemResult()
            {
                Status = "SUCCESS",
                DescriptionEng = $"Your product serial=[{serial}] และ pin=[{pin}] is genuine.",
                DescriptionThai = $"สินค้า ซีเรียล=[{serial}] และ พิน=[{pin}] เป็นของแท้",
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemBySerialPinV2(serial, pin);

            if (result.Result == null)
            {
                r.Status = "NOTFOUND";
                r.DescriptionEng = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";
                r.DescriptionThai = $"ไม่พบ ซีเรียล=[{serial}] และ พิน=[{pin}] ในฐานข้อมูล!!!";

                return r;
            }

            r.ScanItem = result.Result;
            var id = result.Id.ToString();

            if ((result.Result.RegisteredFlag != null) && result.Result.RegisteredFlag!.Equals("TRUE"))
            {
                r.Status = "ALREADY_REGISTERED";
                r.DescriptionEng = $"Your product serial=[{serial}] and pin=[{pin}] is already registered!!!";
                r.DescriptionThai = $"สินค้า ซีเรียล=[{serial}] และ พิน=[{pin}] เคยลงทะเบียนแล้ว!!!";

                repository.IncreaseScanCountV2(id!);

                return r;
            }

            //ถ้าเป็น dryrun ไม่ต้องเรียก RegisterScanItem()
            if (!isDryRun)
            {
                var t = repository.RegisterScanItemV2(id!);
                r.ScanItem = t.Result;
            }

            return r;
        }

        private MVJob? CreateEmailSendOtpJob(string orgId, string serial, string pin, string emailOtp, string email)
        {
            var templateType = "customer-registration-otp";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "ScanItemService.CreateEmailSendOtpJob()",
                Type = "OtpEmailSend",
                Status = "Pending",
                Tags = $"{templateType}",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new NameValue { Name = "OTP", Value = emailOtp },
                    new NameValue { Name = "SERIAL", Value = serial },
                    new NameValue { Name = "PIN", Value = pin },
                ]
            };

            var result = _jobService.AddJob(orgId, job);
            return result;
        }

        private MVJob? ProductRegisterGreetingJob(string orgId, string serial, string pin, string emailOtp, string email)
        {
            var templateType = "customer-registration-welcome";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "ScanItemService.ProductRegisterGreetingJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",
                Tags = $"{templateType}",

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new NameValue { Name = "OTP", Value = emailOtp },
                    new NameValue { Name = "SERIAL", Value = serial },
                    new NameValue { Name = "PIN", Value = pin },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                ]
            };

            var result = _jobService.AddJob(orgId, job);
            return result;
        }

        public MVOtp GetOtpViaEmail(string orgId, string serial, string pin, string otp, string email)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVOtp()
            {
                Status = "SUCCESS",
                Description = "",
            };

            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "GetOtpViaEmail");
            var otpObj = _redis.GetObjectAsync<MOtp>($"{cacheKey}:{serial}:{pin}").Result;
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

            //TODO : Check rate limit here

            //Keep the email sent OTP in cache also for future use
            var emailOtp = ServiceUtils.CreateOTP(6);
            var o = new MOtp()
            {
                Otp = emailOtp,
            };
            var cacheKey2 = CacheHelper.CreateApiOtpKey(orgId, "ReceivedOtpViaEmail");
            _ = _redis.SetObjectAsync($"{cacheKey2}:{serial}:{pin}", o, TimeSpan.FromMinutes(30));

            //Submit job to send OTP email, use serial & pin (masking) in the email
            CreateEmailSendOtpJob(orgId, serial, pin, emailOtp, email);

            r.OTP = emailOtp;
            r.Description = $"OTP [{emailOtp}] sent to email [{email}]";

            return r;
        }

        public MVScanItem AttachScanItemToCustomer(string orgId, string scanItemId, string customerId)
        {
            repository!.SetCustomOrgId(orgId);
            _entityRepo!.SetCustomOrgId(orgId);

            var r = new MVScanItem()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(scanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Scan Item ID [{scanItemId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(customerId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Customer ID [{customerId}] format is invalid";

                return r;
            }

            var customer = _entityRepo.GetEntityById(customerId!);
            if (customer == null)
            {
                r.Status = "CUSTOMER_NOTFOUND";
                r.Description = $"Customer ID [{customer}] not found!!!";

                return r;
            }

            var result = repository!.AttachScanItemToCustomer(scanItemId, customerId, customer);

            r.ScanItem = result;

            return r;
        }

        public MVEntity RegisterCustomer(string orgId, string serial, string pin, string otp, MCustomerRegister cust)
        {
            repository!.SetCustomOrgId(orgId);
            _entityRepo!.SetCustomOrgId(orgId);

            var r = new MVEntity()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "RegisterCustomer");
            var otpObj = _redis.GetObjectAsync<MOtp>($"{cacheKey}:{serial}:{pin}").Result;
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

            var userOtp = cust.EmailOtp;

            var emailSentOtpCacheKey = CacheHelper.CreateApiOtpKey(orgId, "ReceivedOtpViaEmail");
            var emailSentOtpObj = _redis.GetObjectAsync<MOtp>($"{emailSentOtpCacheKey}:{serial}:{pin}").Result;
            if (emailSentOtpObj == null)
            {
                r.Status = "CUSTOMER_OTP_NOTFOUND";
                r.Description = $"OTP [{userOtp}] not found or expire!!!";

                return r;
            }

            if (userOtp != emailSentOtpObj.Otp)
            {
                r.Status = "CUSTOMER_OTP_INVALID";
                r.Description = $"OTP [{userOtp}] invalid (not match)!!!";

                return r;
            }

            var scanItem = repository!.GetScanItemBySerialPinV2(serial, pin).Result;
            if (scanItem == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";

                return r;
            }

            var customerId = scanItem.CustomerId.ToString();
            if (ServiceUtils.IsGuidValid(customerId!))
            {
                r.Status = "SCAN_ITEM_IS_ALREADY_OCCUPIED";
                r.Description = $"Scan Item is already owned by another customer=[{customerId}]!!!";

                return r;
            }

            var entity = new MEntity()
            {
                PrimaryEmail = cust.Email,
                PrimaryEmailStatus = "VERIFIED", //เนื่องจากผ่านการยืนยัน OTP มาแล้ว
                EntityType = 1,
                EntityCategory = 1,
                Code = $"CUST:{Guid.NewGuid()}",
                Name = cust.Email,
            };

            //Get or create customer here
            var customer = _entityRepo.GetOrCreateEntityByEmail(entity);
            customerId = customer.Id.ToString();

            AttachScanItemToCustomer(orgId, scanItem.Id.ToString()!, customerId!);
            ProductRegisterGreetingJob(orgId, serial, pin, userOtp!, cust.Email!);

            CreatePointTriggerJob(orgId, scanItem, customer);

            r.Entity = customer;
            return r;
        }

        private void CreatePointTriggerJob(string orgId, MScanItem sci, MEntity cust)
        {
            _itemRepo.SetCustomOrgId(orgId);
            _pointRepo.SetCustomOrgId(orgId);

            var customerId = cust.Id.ToString()!;
            var t = _pointRepo.GetWalletByCustomerId(customerId); //เอา wallet อันแรกที่เจอมาใช้
            var wallet = t.Result;

            if (wallet == null)
            {
                Log.Warning($"Wallet not found for customer ID [{customerId}], scan item [{sci.Serial}]");
                return;
            }

            MItem? product = null;
            var productId = sci.ItemId.ToString();
            
            if (!string.IsNullOrEmpty(productId))
            {
                //มีการ attach product ไว้แล้ว
                Log.Debug($"Fetching product ID [{productId}], scan item [{sci.Serial}]");
                product = _itemRepo.GetItemById(productId);
            }

            if (product == null)
            {
                //ไม่มี product attach กับ scan item นั้นจริง ๆ
                Log.Warning($"Product not found for product ID [{productId}], scan item [{sci.Serial}]");
                product = new MItem()
                {
                    Code = "",
                    Tags = "",
                };
            }

            var token = Guid.NewGuid().ToString();
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "ScanItemService.CreatePointTriggerJob()",
                Type = "PointTrigger",
                Status = "Pending",
                Tags = "CustomerRegistered",

                Parameters =
                [
                    new NameValue { Name = "TOKEN", Value = token },
                    new NameValue { Name = "SERIAL", Value = sci.Serial },
                    new NameValue { Name = "PIN", Value = sci.Pin },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                    new NameValue { Name = "PRODUCT_CODE", Value = product.Code },
                    new NameValue { Name = "PRODUCT_TAGS", Value = product.Tags },
                    new NameValue { Name = "PRODUCT_QUANTITY", Value = "1" },
                    new NameValue { Name = "WALLET_ID", Value = wallet.Id.ToString() },
                    new NameValue { Name = "EVENT_TRIGGER", Value = "CustomerRegistered" },
                ]
            };

            var cacheKey = CacheHelper.CreatePointTriggerCustRegisterKey(orgId);
            _redis.SetObjectAsync($"{cacheKey}:{token}", job, TimeSpan.FromMinutes(10));

            _jobService.AddJob(orgId, job);
            
            return;
        }

        private string MaskUrl(string pin, string maskPin, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }

            var maskUrl = url.Replace(pin, maskPin);
            return maskUrl;
        }

        public MVScanItem DetachScanItemFromProduct(string orgId, string scanItemId)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVScanItem()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(scanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Scan Item ID [{scanItemId}] format is invalid";

                return r;
            }

            var result = repository!.DetachScanItemFromProduct(scanItemId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Scan Item ID [{scanItemId}] not found for the organization [{orgId}]";
                
                return r;
            }

            r.ScanItem = result;

            return r;
        }

        public MVScanItem DetachScanItemFromCustomer(string orgId, string scanItemId)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVScanItem()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(scanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Scan Item ID [{scanItemId}] format is invalid";

                return r;
            }

            var result = repository!.DetachScanItemFromCustomer(scanItemId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Scan Item ID [{scanItemId}] not found for the organization [{orgId}]";
                
                return r;
            }

            r.ScanItem = result;

            return r;
        }

        public async Task<MVScanItem> GetScanItemUrlDryRunById(string orgId, string scanItemId)
        {
            var r = new MVScanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemByIdV2(scanItemId);

            if (result == null)
            {
                r.Status = "SCAN_ITEM_NOT_FOUND";
                r.Description = $"Scan item ID [{scanItemId}] not found!!!";
                return r;
            }

            //จะไม่ทำการ masking URL ใน API นี้
            result.Pin = "";

            var otp = ServiceUtils.CreateOTP(6);
            var otpObj = new MOtp() { Id = result.Id.ToString(), Otp = otp };
            var token = $"{otpObj.Id}-{otpObj.Otp}";

            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "IsDryRunTokenValid");
            var key = $"{cacheKey}:{token}";
            await _redis.SetObjectAsync(key, otpObj, TimeSpan.FromMinutes(5));

            result.Url = $"{result.Url}?dryrun_token={token}";
            r.ScanItem = result;
            return r;
        }

        //=== V2 ===
        public async Task<int> GetScanItemCountV2(string orgId, VMScanItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemCountV2(param);

            return result;
        }
        
        public async Task<IEnumerable<MScanItem>> GetScanItemsV2(string orgId, VMScanItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemsV2(param);

            //Masking PIN & URL
            result.ToList().ForEach(item =>
            {
                var maskPin = ServiceUtils.MaskScanItemPin(item.Pin!);
                item.Url = MaskUrl(item.Pin!, maskPin, item.Url!);
                item.Pin = maskPin;

                if (!string.IsNullOrEmpty(item.ProductCodeLegacy))
                {
                    //ใช้ของเดิมที่เกาะกับ scan item, แทนที่จะเอามาจาก folder
                    item.ProductCode = item.ProductCodeLegacy;
                    item.ProductDesc = item.ProductDescLegacy;
                }
            });

            return result;
        }

        public async Task<MVScanItem> GetScanItemByIdV2(string orgId, string scanItemId)
        {
            var r = new MVScanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemByIdV2(scanItemId);

            var maskPin = ServiceUtils.MaskScanItemPin(result.Pin!);
            result.Url = MaskUrl(result.Pin!, maskPin, result.Url!);
            result.Pin = maskPin;

            if (!string.IsNullOrEmpty(result.ProductCodeLegacy))
            {
                //ใช้ของเดิมที่เกาะกับ scan item, แทนที่จะเอามาจาก folder
                result.ProductCode = result.ProductCodeLegacy;
                result.ProductDesc = result.ProductDescLegacy;
            }

            r.ScanItem = result;
            return r;
        }

        public async Task<MVScanItem> AddScanItemV2(string orgId, MScanItem scanItem)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            var pinExist = await repository!.IsPinExistV2(scanItem.Pin!);
            if (pinExist)
            {
                r.Status = "PIN_ALREADY_EXIST";
                r.Description = $"Pin [{scanItem.Pin}] already exist in our database!!!";

                return r;
            }

            var serialExist = await repository!.IsSerialExistV2(scanItem.Serial!);
            if (serialExist)
            {
                r.Status = "SERIAL_ALREADY_EXIST";
                r.Description = $"Serial [{scanItem.Serial}] already exist in our database!!!";

                return r;
            }

            //สร้าง URL ให้อัตโนมัติเลย
            var m = await _sciTemplateSvc!.GetScanItemTemplate_V2(orgId);
            if (m == null)
            {
                r.Status = "NO_SCAN_ITEM_TEMPLATE_FOUND";
                r.Description = $"No scan item template found!!!";

                return r;
            }

            if (string.IsNullOrEmpty(m.UrlTemplate))
            {
                r.Status = "URL_TEMPLATE_EMPTY";
                r.Description = $"Scan item template URL is empty!!!";

                return r;
            }

            var url = m.UrlTemplate!;
            url = url.Replace("{VAR_ORG}", orgId);
            url = url.Replace("{VAR_SERIAL}", scanItem.Serial);
            url = url.Replace("{VAR_PIN}", scanItem.Pin);
            scanItem.Url = url;

            var result = await repository!.AddScanItemV2(scanItem);
            r.ScanItem = result;

            return r;
        }

        public async Task<MVScanItem> DeleteScanItemByIdV2(string orgId, string scanItemId)
        {
            var r = new MVScanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(scanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Scan Item ID [{scanItemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = await repository!.DeleteScanItemByIdV2(scanItemId);

            r.ScanItem = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Scan Item ID [{scanItemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public async Task<MVScanItem> UnVerifyScanItemByIdV2(string orgId, string scanItemId)
        {
            var r = new MVScanItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(scanItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Scan Item ID [{scanItemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = await repository!.UnVerifyScanItemByIdV2(scanItemId);

            r.ScanItem = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Scan Item ID [{scanItemId}] not found for the organization [{orgId}]";
            }

            return r;
        }
    }
}
