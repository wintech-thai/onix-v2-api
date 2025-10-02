using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ScanItemTemplateService : BaseService, IScanItemTemplateService
    {
        private readonly IScanItemTemplateRepository? repository = null;
        private readonly IRedisHelper _redis;
        private readonly IUserRepository _userRepo;

        public ScanItemTemplateService(IScanItemTemplateRepository repo, IRedisHelper redis, IUserRepository userRepo) : base()
        {
            repository = repo;
            _redis = redis;
            _userRepo = userRepo;
        }

        public MScanItemTemplate GetScanItemTemplateById(string orgId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemTemplateById(actionId);

            return result;
        }

        public MScanItemTemplate GetScanItemTemplate(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemTemplate();

            return result;
        }

        public MScanItemTemplate GetScanItemTemplateDefault(string orgId, string userName)
        {
            var scanDomain = "scan";

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                scanDomain = "scan-dev";
            }

            //เพื่อป้องกัน string interpolate
            var serial = "{VAR_SERIAL}";
            var pin = "{VAR_PIN}";

            var email = "your-email@email-xxx.com";
            _userRepo.SetCustomOrgId(orgId);

            if (!string.IsNullOrEmpty(userName))
            {
                //หา email ของ user คนนั้นเพื่อใส่เป็นค่า default
                var user = _userRepo.GetUserByName(userName);
                if (user != null)
                {
                    email = user.UserEmail!;
                }
            }

            var t = new MScanItemTemplate()
            {
                SerialPrefixDigit = 2,
                GeneratorCount = 100,
                SerialDigit = 7,
                PinDigit = 7,
                UrlTemplate = $"https://{scanDomain}.please-scan.com/org/{orgId}/Verify/{serial}/{pin}",
                NotificationEmail = email
            };

            return t;
        }

        public MVScanItemTemplate? AddScanItemTemplate(string orgId, MScanItemTemplate action)
        {
            //TODO : Validate email format

            //Allow only 1 in organization
            var param = new VMScanItemTemplate()
            {
                FullTextSearch = ""
            };

            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemTemplate();
            r.Status = "OK";
            r.Description = "Success";

            var actionCount = GetScanItemTemplateCount(orgId, param);
            if (actionCount > 0)
            {
                r.Status = "NOT_ALLOW_MORE_THAN_ONE";
                r.Description = $"Found more than 1 scan-item ({actionCount}) template in organization [{orgId}]";

                return r;
            }

            var result = repository!.AddScanItemTemplate(action);
            r.ScanItemTemplate = result;

            return r;
        }

        public MVScanItemTemplate? UpdateScanItemTemplateById(string orgId, string actionId, MScanItemTemplate action)
        {
            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateScanItemTemplateById(actionId, action);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemTemplate ID [{actionId}] not found for the organization [{orgId}]";

                return r;
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemTemplateKey(orgId));

            r.ScanItemTemplate = result;
            return r;
        }

        public MVScanItemTemplate? DeleteScanItemTemplateById(string orgId, string actionId)
        {
            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemTemplate ID [{actionId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteScanItemTemplateById(actionId);

            r.ScanItemTemplate = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemTemplate ID [{actionId}] not found for the organization [{orgId}]";
            }

            _redis.DeleteAsync(CacheHelper.CreateScanItemTemplateKey(orgId));
            return r;
        }

        public int GetScanItemTemplateCount(string orgId, VMScanItemTemplate param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemTemplateCount(param);

            return result;
        }
    }
}
