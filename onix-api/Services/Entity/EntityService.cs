using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using System.Text;
using System.Web;

namespace Its.Onix.Api.Services
{
    public class EntityService : BaseService, IEntityService
    {
        private readonly IEntityRepository? repository = null;
        private readonly IRedisHelper _redis;
        private readonly IJobService _jobService;

        public EntityService(
            IEntityRepository repo,
            IJobService jobService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
            _jobService = jobService;
        }

        public MEntity GetEntityById(string orgId, string entityId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetEntityById(entityId);

            return result;
        }

        public MVEntity? AddEntity(string orgId, MEntity entity)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVEntity();

            var isCodeExist = repository!.IsEntityCodeExist(entity.Code!);
            if (isCodeExist)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Entity code [{entity.Code}] is duplicate";

                return r;
            }

            //ที่ต้องให้ email unique เพราะใช้ email ตอนลงทะเบียนในหน้า verify เพื่อผูก scan item กับ customer
            var isEmailExist = repository!.IsPrimaryEmailExist(entity.PrimaryEmail!);
            if (isEmailExist)
            {
                r.Status = "EMAIL_DUPLICATE";
                r.Description = $"Email [{entity.PrimaryEmail}] is duplicate";

                return r;
            }

            var result = repository!.AddEntity(entity);

            r.Status = "OK";
            r.Description = "Success";
            r.Entity = result;

            return r;
        }

        private MVJob? CreateEmailCustomerVerificationJob(string orgId, MEmailVerification reg)
        {
            var regType = "customer-email-virification";

            var jsonString = JsonSerializer.Serialize(reg);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);

            var dataUrlSafe = HttpUtility.UrlEncode(jsonStringB64);

            var registerDomain = "register";
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                registerDomain = "register-dev";
            }

            var token = Guid.NewGuid().ToString();
            var registrationUrl = $"https://{registerDomain}.please-scan.com/{orgId}/{regType}/{token}?data={dataUrlSafe}";

            var templateType = "customer-email-verification";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "Entity.CreateEmailCustomerVerifyJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",
                Tags = templateType,

                Parameters =
                [
                    new NameValue { Name = "EMAIL_NOTI_ADDRESS", Value = "pjame.fb@gmail.com" },
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = reg.Email },
                    new NameValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new NameValue { Name = "USER_ORG_ID", Value = orgId },
                    new NameValue { Name = "REGISTRATION_URL", Value = registrationUrl },
                ]
            };

            var result = _jobService.AddJob(orgId, job);

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "CustomerEmailVerification");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return result;
        }
        
        public MVEntity? UpdateEntityEmailById(string orgId, string entityId, string email, bool sendVerification)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVEntity()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(entityId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Entity ID [{entityId}] format is invalid";

                return r;
            }

            var emailValidation = ValidationUtils.ValidateEmail(email);
            if (emailValidation.Status != "OK")
            {
                r.Status = emailValidation.Status;
                r.Description = emailValidation.Description;
                return r;
            }

            var en = repository!.GetEntityByEmail(email);
            if ((en != null) && (en.Id.ToString() != entityId))
            {
                //ตรงนี้ คือ มี entity อื่นที่ใช้ email นี้อยู่แล้ว
                r.Status = "EMAIL_DUPLICATE";
                r.Description = $"Email [{email}] is duplicate";

                return r;
            }

            if (en != null && en.Id.ToString() == entityId)
            {
                //ตรงนี้ คือ email เดิมที่ใช้กับ entity ตัวนี้อยู่แล้ว
                r.Status = "EMAIL_SAMEASBEFORE";
                r.Description = $"Email [{email}] is same as before";

                return r;
            }

            var result = repository!.UpdateEntityEmailById(entityId, email);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Entity ID [{entityId}] not found for the organization [{orgId}]";

                return r;
            }

            if (sendVerification)
            {
                //Send verification email
                var reg = new MEmailVerification()
                {
                    Id = result.Id.ToString(),
                    Code = result.Code,
                    Name = result.Name,
                    Email = email,
                };
                CreateEmailCustomerVerificationJob(orgId, reg);
            }

            r.Entity = result;
            return r;
        }

        public MVEntity? UpdateEntityById(string orgId, string entityId, MEntity cycle)
        {
            var r = new MVEntity()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(entityId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Entity ID [{entityId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateEntityById(entityId, cycle);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Entity ID [{entityId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Entity = result;
            return r;
        }

        public MVEntity? DeleteEntityById(string orgId, string entityId)
        {
            var r = new MVEntity()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(entityId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Entity ID [{entityId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteEntityById(entityId);

            r.Entity = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Entity ID [{entityId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MEntity> GetEntities(string orgId, VMEntity param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetEntities(param);

            foreach (var entity in result)
            {
                //เพื่อไม่ให้ข้อมูลที่ response กลับไปใหญ่จนเกินไป
                entity.Content = "";
            }

            return result;
        }

        public int GetEntityCount(string orgId, VMEntity param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetEntityCount(param);

            return result;
        }
    }
}
