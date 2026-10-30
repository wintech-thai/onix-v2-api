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

        // ออก otp ใหม่ + ขยายวันหมดอายุ, adminDomain ถ้าส่งมาจะอัปเดตด้วย
        public Task<MServiceRequest?> RegenerateOtp(string serviceRequestId, string newOtp, DateTime newExpiredDate, string? adminDomain = null);

        public Task<MServiceRequest?> DeleteServiceRequestById(string serviceRequestId);
        public Task<IEnumerable<MServiceRequest>> GetServiceRequests(VMServiceRequest param);
        public Task<int> GetServiceRequestCount(VMServiceRequest param);

        // x089 ปรับแก้ 8.a : เช็คชนกับ ServiceRequest แถวอื่น ไม่ใช่แค่ MMerchant
        public Task<bool> IsShopCodeExist(string shopCode, Guid excludeId);

        // x089 Bug 7 : เช็คชนกับ ServiceRequest แถวอื่นที่ยัง Pending/Confirmed อยู่ ไม่ใช่แค่ MUser (ที่ยังไม่มีจนกว่าจะ Approve)
        public Task<bool> IsEmailExist(string email);
    }
}
