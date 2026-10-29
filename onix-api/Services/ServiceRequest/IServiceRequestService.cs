using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IServiceRequestService
    {
        // x089 7.a.i : request มาจาก please-payment.com (ผ่าน API key) — PleasePayment-specific,
        // ServiceType ถูก hardcode เป็น CustomerRegistration เสมอ ผลิตภัณฑ์อื่น (PleaseScan,
        // PleaseERP) จะมี Create*RegistrationLink ของตัวเองแยกต่างหาก
        public Task<MVServiceRequest> CreatePleasePaymentRegistrationLink(MServiceRequest request);

        // x089 7.a.ii : ไม่มี authen, ใช้ requestId+otp พิสูจน์สิทธิ์แทน
        public Task<MVServiceRequest> ConfirmPleasePaymentServiceRequest(string requestId, string otp, MServiceRequest request);

        // ใช้ทั้งตอนเปิดหน้า customer-signup (prefill) และก่อน upload book bank — ไม่มี authen เช่นกัน
        public Task<MVServiceRequest> GetServiceRequestByIdAndOtp(string requestId, string otp);

        public Task<MVServiceRequest> UploadServiceRequestBookBank(string requestId, string otp, MFileDocument fileDocument);

        // x089 7.a.iii : CRUD ปกติ ฝั่ง Admin (มี authen)
        public Task<MVServiceRequest> GetServiceRequestById(string serviceRequestId);
        public Task<IEnumerable<MServiceRequest>> GetServiceRequests(VMServiceRequest param);
        public Task<int> GetServiceRequestCount(VMServiceRequest param);
        public Task<MVServiceRequest> UpdateServiceRequestById(string serviceRequestId, MServiceRequest request);
        public Task<MVServiceRequest> DeleteServiceRequestById(string serviceRequestId);

        // ปุ่ม regenerate link / resend email บนหน้า Admin
        public Task<MVServiceRequest> RegenerateRegistrationLink(string serviceRequestId, string? adminDomain = null);
        public Task<MVServiceRequest> ResendRegistrationLinkEmail(string serviceRequestId);
    }
}
