using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IServiceRequestRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<MServiceRequest> AddServiceRequest(MServiceRequest serviceRequest);
        public Task<MServiceRequest?> GetServiceRequestById(string serviceRequestId);
        public Task<MServiceRequest?> GetServiceRequestByIdAndOtp(string serviceRequestId, string otp);

        // Callers must decide which fields are allowed to change for the given caller
        // (public confirm vs admin edit) before calling this — it persists whatever is
        // already set on the passed-in object onto every editable column.
        public Task<MServiceRequest?> UpdateServiceRequestById(string serviceRequestId, MServiceRequest serviceRequest);

        // ออก otp ใหม่ + ขยายวันหมดอายุ
        public Task<MServiceRequest?> RegenerateOtp(string serviceRequestId, string newOtp, DateTime newExpiredDate);

        public Task<MServiceRequest?> DeleteServiceRequestById(string serviceRequestId);
        public Task<IEnumerable<MServiceRequest>> GetServiceRequests(VMServiceRequest param);
        public Task<int> GetServiceRequestCount(VMServiceRequest param);
    }
}
