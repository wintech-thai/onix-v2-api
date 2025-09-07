using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class JobService : BaseService, IJobService
    {
        private readonly IJobRepository? repository = null;

        public JobService(IJobRepository repo) : base()
        {
            repository = repo;
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

            var result = repository!.AddJob(job);

            r.Status = "OK";
            r.Description = "Success";
            r.Job = result;

            //TODO : Publish job to message queue here...

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
