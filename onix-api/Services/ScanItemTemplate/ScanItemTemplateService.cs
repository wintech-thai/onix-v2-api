using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using System.Data.SqlClient;

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

        public async Task<MVScanItemTemplate> GetScanItemTemplateById_V2(string orgId, string templateId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(templateId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemTemplate ID [{templateId}] format is invalid";

                return r;
            }

            var result = await repository!.GetScanItemTemplateById_V2(templateId);
            r.ScanItemTemplate = result;

            return r;
        }

        public async Task<MScanItemTemplate?> GetScanItemTemplate_V2(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemTemplate_V2();

            return result;
        }

        private MVScanItemTemplate ValidateTemplate(MScanItemTemplate template)
        {
            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success",
            };

            if (template.GeneratorCount > 10000)
            {
                r.Status = "ITEM_COUNT_TOO_BIG";
                r.Description = "Item count is above limit, limit is 10,000";
                return r;
            }

            if ((template.PinDigit > 7) || (template.PinDigit < 5))
            {
                r.Status = "PIN_DIGIT_INVALID";
                r.Description = "PIN must be 5-7 digits";
                return r;
            }

            if ((template.SerialPrefixDigit > 3) || (template.PinDigit < 2))
            {
                r.Status = "SERIAL_PREFIX_INVALID";
                r.Description = "Serial number prefix must be 2-3 digits";
                return r;
            }

            if ((template.SerialDigit > 7) || (template.SerialDigit < 6))
            {
                r.Status = "SERIAL_DIGIT_INVALID";
                r.Description = "Serial number must be 6-7 digits";
                return r;
            }

            var email = template.NotificationEmail;
            if (email == null)
            {
                //เพื่อให้ validate error
                email = "";
            }

            var emailValidateResult = ValidationUtils.ValidateEmail(email);
            if (emailValidateResult.Status != "OK")
            {
                r.Status = emailValidateResult.Status;
                r.Description = emailValidateResult.Description;

                return r;
            }

            return r;
        }

        public async Task<MVScanItemTemplate> AddScanItemTemplate_V2(string orgId, MScanItemTemplate template)
        {
            //TODO : ให้ validate ค่าพวก length digit
            repository!.SetCustomOrgId(orgId);
            template.IsDefault = "NO";

            var r = new MVScanItemTemplate();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(template.TemplateName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Action name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsScanItemTemplateExist(template.TemplateName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Action name [{template.TemplateName}] already exist!!!";

                return r;
            }

            var validateResult = ValidateTemplate(template);
            if (validateResult.Status != "OK")
            {
                return validateResult;
            }

            var result = await repository!.AddScanItemTemplate_V2(template);
            r.ScanItemTemplate = result;

            return r;
        }

        public async Task<MVScanItemTemplate> DeleteScanItemTemplateById_V2(string orgId, string templateId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(templateId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemTemplate ID [{templateId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteScanItemTemplateById_V2(templateId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemTemplate ID [{templateId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemTemplate = m;
            return r;
        }

        public async Task<int> GetScanItemTemplateCount_V2(string orgId, VMScanItemTemplate param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemTemplateCount_V2(param);

            return result;
        }

        public async Task<List<MScanItemTemplate>> GetScanItemTemplates_V2(string orgId, VMScanItemTemplate param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemTemplates_V2(param);

            return result;
        }

        public async Task<MVScanItemTemplate> UpdateScanItemTemplateById_V2(string orgId, string templateId, MScanItemTemplate template)
        {
            //TODO : Check if template name is duplicate
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(templateId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemTemplate ID [{templateId}] format is invalid";

                return r;
            }

            var validateResult = ValidateTemplate(template);
            if (validateResult.Status != "OK")
            {
                return validateResult;
            }

            var result = await repository!.UpdateScanItemTemplateById_V2(templateId, template);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItemTemplate ID [{templateId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemTemplate = result;
            return r;
        }

        public async Task<MVScanItemTemplate> SetDefaultScanItemTemplateById_V2(string orgId, string templateId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemTemplate()
            {
                Status = "OK",
                Description = "Success"
            };

            var result = await repository!.SetScanItemTemplateDefault_V2(templateId);
            r.ScanItemTemplate = result;

            return r;
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

        public async Task<MVJob> GetJobDefaultByTemplateId(string orgId, string jobType, string userName, string templateId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVJob()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(templateId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Template ID [{templateId}] format is invalid";

                return r;
            }

            var template = await repository!.GetScanItemTemplateById_V2(templateId);
            if (template == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItem Template ID [{templateId}] not found for the organization [{orgId}]";

                return r;
            }

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

            var parameters = new[]
            {
                new { Name = "EMAIL_NOTI_ADDRESS", Value = email },
                new { Name = "SCAN_ITEM_COUNT", Value = $"{template.GeneratorCount}" },
                new { Name = "SERIAL_NUMBER_DIGIT", Value = $"{template.SerialDigit}" },
                new { Name = "SERIAL_NUMBER_PREFIX_DIGIT", Value = $"{template.SerialPrefixDigit}" },
                new { Name = "PIN_DIGIT", Value = $"{template.PinDigit}" },
            };

            var jobKey = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var job = new MJob()
            {
                Name = $"{jobType}-{jobKey}",
                Description = $"Job to generate Scan Items",
            };

            foreach (var p in parameters)
            {
                var o = new NameValue() { Name = p.Name, Value = p.Value };
                job.Parameters.Add(o);
            }

            r.Job = job;

            return r;
        }
    }
}
