using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IJobService
    {
        public MJob? GetJobById(string orgId, string itemId);
        public MJob? GetJobByRefId(string orgId, string refId);
        public MJob GetJobTemplate(string orgId, string jobType, string userName);
        public MVJob? AddJob(string orgId, MJob item, bool triggerJob = true);
        public IEnumerable<MJob> GetJobs(string orgId, VMJob param);
        public int GetJobCount(string orgId, VMJob param);
        public MVJob? DeleteJobById(string orgId, string jobId);
        public MJob CreatePayOutSuccessJob(string orgId, string jobType, MPaymentTransaction pmt, MPaymentRequest pmr);
        public MJob CreatePayInRejectedJob(string orgId, string jobType, MPaymentRequest pmr, bool triggerJob = false);
        public MJob CreatePayInRequestedJob(string orgId, string jobType, MPaymentRequest pmr, bool triggerJob = true);
        public MJob CreatePayOutRejectedJob(string orgId, string jobType, MPaymentRequest pmr, bool triggerJob = false);
        public MJob CreatePaymentInSuccessJob(string orgId, string jobType, MPaymentTransaction pmt, MPaymentRequest pmr);
        public MVJob? CloseBackupJob(string orgId, string jobId);
    }
}
