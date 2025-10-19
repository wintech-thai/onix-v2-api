using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IJobRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MJob AddJob(MJob cycle);
        public int GetJobCount(VMJob param);
        public IEnumerable<MJob> GetJobs(VMJob param);
        public MJob GetJobById(string cycleId);
        public MJob? DeleteJobById(string jobId);
    }
}
