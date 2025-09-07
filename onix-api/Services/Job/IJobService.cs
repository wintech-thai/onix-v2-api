using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IJobService
    {
        public MJob GetJobById(string orgId, string itemId);
        public MVJob? AddJob(string orgId, MJob item);
        public IEnumerable<MJob> GetJobs(string orgId, VMJob param);
        public int GetJobCount(string orgId, VMJob param);
    }
}
