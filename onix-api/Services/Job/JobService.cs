using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class JobService : BaseService, IJobService
    {
        private readonly IJobRepository? repository = null;
        private readonly IRedisHelper _redis;
        private readonly IUserRepository _userRepo;

        public JobService(IJobRepository repo, IRedisHelper redis, IUserRepository userRepo) : base()
        {
            repository = repo;
            _redis = redis;
            _userRepo = userRepo;
        }

        public MJob? GetJobById(string orgId, string jobId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobById(jobId);

            if (result != null)
            {
                var parameters = JsonSerializer.Deserialize<List<NameValue>>(result.Configuration!);
                if (parameters == null)
                {
                    parameters = new List<NameValue>();
                }

                result.Parameters = parameters;
                result.Configuration = "";
            }

            return result;
        }

        public MJob GetJobTemplate(string orgId, string jobType, string userName)
        {
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
                new { Name = "SCAN_ITEM_COUNT", Value = "100" },
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

            return job;
        }

        private string? GetEmail(MJob job, string varName)
        {
            foreach (var parm in job.Parameters)
            {
                if (parm.Name == varName)
                {
                    return parm.Value;
                }
            }

            return null;
        }

        public MVJob? AddJob(string orgId, MJob job)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVJob();
            r.Status = "OK";
            r.Description = "Success";

            var email = GetEmail(job, "EMAIL_NOTI_ADDRESS");
            if (email != null)
            {
                var emailValidateResult = ValidationUtils.ValidateEmail(email);
                if (emailValidateResult.Status != "OK")
                {
                    r.Status = emailValidateResult.Status;
                    r.Description = emailValidateResult.Description;

                    return r;
                }
            }

            job.Configuration = JsonSerializer.Serialize(job.Parameters);
            var result = repository!.AddJob(job);
            result.Configuration = "";

            r.Job = result;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"JobSubmitted:{environment}:{job.Type}";
            var message = JsonSerializer.Serialize(r.Job);

            _ = _redis.PublishMessageAsync(stream!, message);

            return r;
        }

        public IEnumerable<MJob> GetJobs(string orgId, VMJob param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobs(param);

            return result;
        }

        public int GetJobCount(string orgId, VMJob param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobCount(param);

            return result;
        }
    }
}
