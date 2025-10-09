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

        public MScanItemAction GetScanItemActionById(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemActionById(actionId);

            return result;
        }

        public MScanItemAction GetScanItemAction(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemAction();

            return result;
        }

        public MScanItemAction GetScanItemActionDefault(string orgId)
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
                RegisteredAwareFlag = "TRUE"
            };

            return action;
        }

        public MVScanItemAction? AddScanItemAction(string orgId, MScanItemAction action)
        {
            var r = new MVScanItemAction();
            r.Status = "OK";
            r.Description = "Success";

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

            //Allow only 1 in organization
            var param = new VMScanItemAction()
            {
                FullTextSearch = ""
            };

            repository!.SetCustomOrgId(orgId);

            var actionCount = GetScanItemActionCount(orgId, param);
            if (actionCount > 0)
            {
                r.Status = "NOT_ALLOW_MORE_THAN_ONE";
                r.Description = $"Found more than 1 scan-item ({actionCount}) action in organization [{orgId}]";

                return r;
            }

            var result = repository!.AddScanItemAction(action);
            r.ScanItemAction = result;

            //ตัว verify เป็นคนใช้ cache ตรงนี้
            var cacheLoaderKey = CacheHelper.CreateScanItemActionCacheLoaderKey(orgId);
            var ec = new CacheLoaderEncryptionConfig() { Encryption_Key = action.EncryptionKey, Encryption_Iv = action.EncryptionIV };
            _redis.SetObjectAsync(cacheLoaderKey, ec);

            return r;
        }

        public MVScanItemAction? UpdateScanItemActionById(string orgId, string actionId, MScanItemAction action)
        {
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

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateScanItemActionById(actionId, action);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemAction ID [{actionId}] not found for the organization [{orgId}]";

                return r;
            }

            //Controller เป็นคนใช้อันนี้ตอน verify scan item
            _redis.DeleteAsync(CacheHelper.CreateScanItemActionKey(orgId));

            //ตัว verify เป็นคนใช้ cache ตรงนี้
            var cacheLoaderKey = CacheHelper.CreateScanItemActionCacheLoaderKey(orgId);
            var ec = new CacheLoaderEncryptionConfig() { Encryption_Key = action.EncryptionKey, Encryption_Iv = action.EncryptionIV };
            _redis.SetObjectAsync(cacheLoaderKey, ec);

            r.ScanItemAction = result;
            return r;
        }

        public MVScanItemAction? DeleteScanItemActionById(string orgId, string actionId)
        {
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

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteScanItemActionById(actionId);

            r.ScanItemAction = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemAction ID [{actionId}] not found for the organization [{orgId}]";
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemActionKey(orgId));
            return r;
        }

        public int GetScanItemActionCount(string orgId, VMScanItemAction param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemActionCount(param);

            return result;
        }
    }
}
