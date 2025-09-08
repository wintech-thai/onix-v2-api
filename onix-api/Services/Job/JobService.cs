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
        private readonly RedisHelper _redis;

        public JobService(IJobRepository repo, RedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public MJob GetJobById(string orgId, string jobId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobById(jobId);

            return result;
        }

        public MVJob? AddJob(string orgId, MJob job)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVJob();
            r.Status = "OK";
            r.Description = "Success";

            job.Configuration = JsonSerializer.Serialize(job.Parameters);
            var result = repository!.AddJob(job);
            result.Configuration = "";
            
            r.Job = result;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"{environment}:{job.Type}";
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
