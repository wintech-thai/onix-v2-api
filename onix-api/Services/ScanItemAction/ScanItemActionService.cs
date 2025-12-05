using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class CacheLoaderEncryptionConfig
    {
        public string? Encryption_Key { get; set; }
        public string? Encryption_Iv { get; set; }
    }

    public class ScanItemActionService : BaseService, IScanItemActionService
    {
        private readonly IScanItemActionRepository? repository = null;
        private readonly IRedisHelper _redis;

        public ScanItemActionService(IScanItemActionRepository repo, IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public async Task<MVScanItemAction> GetScanItemActionById_V2(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemAction()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemAction ID [{actionId}] format is invalid";

                return r;
            }

            var result = await repository!.GetScanItemActionById_V2(actionId);
            r.ScanItemAction = result;

            return r;
        }

        public async Task<MScanItemAction?> GetScanItemAction_V2(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetDefaultScanItemAction_V2();

            return result;
        }

        public MScanItemAction GetScanItemActionDefault_V2(string orgId)
        {
            var verifyDomain = "verify";

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                verifyDomain = "verify-dev";
            }

            var action = new MScanItemAction()
            {
                RedirectUrl = $"https://{verifyDomain}.please-scan.com/verify",
                EncryptionKey = ServiceUtils.GenerateSecureRandomString(16),
                EncryptionIV = ServiceUtils.GenerateSecureRandomString(16),
                ThemeVerify = "default",
                RegisteredAwareFlag = "TRUE",
                IsDefault = "FALSE",
            };

            return action;
        }

        public async Task<MVScanItemAction> AddScanItemAction_V2(string orgId, MScanItemAction action)
        {
            repository!.SetCustomOrgId(orgId);
            action.IsDefault = "NO";

            var r = new MVScanItemAction();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(action.ActionName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Action name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsScanItemActionExist(action.ActionName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Action name [{action.ActionName}] already exist!!!";

                return r;
            }

            var keyValidationResult = ValidationUtils.ValidateKeyAndIV(action.EncryptionKey);
            if (keyValidationResult.Status != "OK")
            {
                r.Status = keyValidationResult.Status;
                r.Description = keyValidationResult.Description;

                return r;
            }

            var ivValidationResult = ValidationUtils.ValidateKeyAndIV(action.EncryptionIV);
            if (ivValidationResult.Status != "OK")
            {
                r.Status = ivValidationResult.Status;
                r.Description = ivValidationResult.Description;

                return r;
            }

            var result = await repository!.AddScanItemAction_V2(action);
            r.ScanItemAction = result;

            var actionId = result.Id.ToString();
            /* Console.WriteLine($"@@@@@ DEBUGxxx Action ID = [{actionId}]"); */

            //ตัว verify เป็นคนใช้ cache ตรงนี้
            var cacheLoaderKey = CacheHelper.CreateScanItemActionCacheLoaderKey_V2(orgId, actionId!);
            var ec = new CacheLoaderEncryptionConfig() { Encryption_Key = action.EncryptionKey, Encryption_Iv = action.EncryptionIV };
            await _redis.SetObjectAsync(cacheLoaderKey, ec);

            //ไม่ต้อง add cache ให้กับ Controller ใช้เพราะว่า เดี่ยวจะไปโหลดใหม่เองตอนที่ VerifyScanItem()

            return r;
        }

        public async Task<MVScanItemAction> DeleteScanItemActionById_V2(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemAction()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemAction ID [{actionId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteScanItemActionById_V2(actionId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemAction ID [{actionId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemAction = m;

            var cacheLoaderKey1 = CacheHelper.CreateScanItemActionCacheLoaderKey_V2(orgId, actionId!);
            await _redis.DeleteAsync(cacheLoaderKey1);

            if (m.IsDefault == "YES")
            {
                //ให้ลบ cache ของตัว default ด้วย
                var cacheLoaderKey2 = CacheHelper.CreateScanItemActionCacheLoaderKey(orgId);
                await _redis.DeleteAsync(cacheLoaderKey2);
            }

            return r;
        }

        public async Task<int> GetScanItemActionCount_V2(string orgId, VMScanItemAction param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemActionsCount_V2(param);

            return result;
        }

        public async Task<List<MScanItemAction>> GetScanItemActions_V2(string orgId, VMScanItemAction param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemActions_V2(param);

            return result;
        }

        public async Task<MVScanItemAction> UpdateScanItemActionById_V2(string orgId, string actionId, MScanItemAction action)
        {
            //TODO : Check if action name is duplicate
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemAction()
            {
                Status = "OK",
                Description = "Success"
            };

            var keyValidationResult = ValidationUtils.ValidateKeyAndIV(action.EncryptionKey);
            if (keyValidationResult.Status != "OK")
            {
                r.Status = keyValidationResult.Status;
                r.Description = keyValidationResult.Description;

                return r;
            }

            var ivValidationResult = ValidationUtils.ValidateKeyAndIV(action.EncryptionIV);
            if (ivValidationResult.Status != "OK")
            {
                r.Status = ivValidationResult.Status;
                r.Description = ivValidationResult.Description;

                return r;
            }

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemAction ID [{actionId}] format is invalid";

                return r;
            }
            
            var result = await repository!.UpdateScanItemActionById_V2(actionId, action);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemAction ID [{actionId}] not found for the organization [{orgId}]";

                return r;
            }

            if (result.IsDefault == "YES")
            {
                //ให้ลบ cache ของตัว default ด้วย เพื่อให้มีการ load ใหม่ทันทีที่ Controller
                var cacheLoaderKey0 = CacheHelper.CreateScanItemActionCacheLoaderKey(orgId);
                await _redis.DeleteAsync(cacheLoaderKey0);
            }

            //Controller เป็นคนใช้อันนี้ตอน verify scan item
            var cacheLoaderKey1 = CacheHelper.CreateScanItemActionKey_V2(orgId, actionId);
            await _redis.DeleteAsync(cacheLoaderKey1);

            //ตัว verify เป็นคนใช้ cache ตรงนี้
            var cacheLoaderKey2 = CacheHelper.CreateScanItemActionCacheLoaderKey_V2(orgId, actionId);
            var ec = new CacheLoaderEncryptionConfig() { Encryption_Key = action.EncryptionKey, Encryption_Iv = action.EncryptionIV };
            await _redis.SetObjectAsync(cacheLoaderKey2, ec);

            r.ScanItemAction = result;
            return r;
        }

        public async Task<MVScanItemAction> SetDefaultScanItemActionById_V2(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemAction()
            {
                Status = "OK",
                Description = "Success"
            };

            var result = await repository!.SetScanItemActionDefault_V2(actionId);
            r.ScanItemAction = result;

            if (result != null)
            {
                //ให้ update cache ของตัว default ด้วย, ตัว Verify เป็นคนใช้ตรงนี้
                var cacheLoaderKey1 = CacheHelper.CreateScanItemActionCacheLoaderKey(orgId);
                var ec = new CacheLoaderEncryptionConfig() { Encryption_Key = result.EncryptionKey, Encryption_Iv = result.EncryptionIV };
                await _redis.SetObjectAsync(cacheLoaderKey1, ec);
            }

            //Controller เป็นคนใช้อันนี้ตอน verify scan item
            //ตัว default
            await _redis.DeleteAsync(CacheHelper.CreateScanItemActionKey(orgId));

            return r;
        }
    }
}
