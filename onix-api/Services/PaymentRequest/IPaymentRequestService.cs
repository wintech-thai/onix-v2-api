using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPaymentRequestService
    {
        public Task<MVPaymentRequest> GetPaymentRequestById(string orgId, string paymentRequestId);

        public Task<MVPaymentResponse> AddPaymentRequestPayIn(string orgId, MPaymentRequest paymentRequest, MMerchant merchant);

        public Task<MVPaymentResponse> AddPaymentRequestPayInP2P(string orgId, MPaymentRequest paymentRequest, MMerchant merchant);

        public Task<MVPaymentRequest> AddPaymentRequestPayOut(string orgId, MPaymentRequest paymentRequest, MMerchant merchant, MBankAccount bankAccount);
        public Task<MVPaymentRequest> AddPaymentRequestTransfer(string orgId, MPaymentRequest paymentRequest, MBankAccount destBa, MBankAccount srcBa);

        public Task<List<MPaymentRequest>> GetPaymentRequests(string orgId, VMPaymentRequest param);
        public Task<int> GetPaymentRequestCount(string orgId, VMPaymentRequest param);

        public Task<MVPaymentRequest> UpdatePaymentRequestById(string orgId, string paymentRequestId, MPaymentRequest paymentRequest);
        
        public Task<MVPaymentRequest> UpdatePaymentRequestPayOut(string orgId, string paymentRequestId, MPaymentRequest paymentRequest, MBankAccount bankAccount, MMerchant merchant);
        public Task<MVPaymentRequest> RejectPaymentRequestPayOut(string orgId, string paymentRequestId, MPaymentRequest paymentRequest);
        public Task<MVPaymentRequest> ApprovePaymentRequestPayOut(string orgId, string paymentRequestId, MPaymentRequest paymentRequest);

        public Task<MVPaymentRequest> UpdatePaymentRequestTransfer(string orgId, string paymentRequestId, MPaymentRequest paymentRequest, MBankAccount srcBa);
        public Task<MVPaymentRequest> RejectPaymentRequestTransfer(string orgId, string paymentRequestId, MPaymentRequest paymentRequest);
        public Task<MVPaymentRequest> ApprovePaymentRequestTransfer(string orgId, string paymentRequestId, MPaymentRequest paymentRequest);

        public Task<MVPaymentRequest> DeletePayOutRequestById(string orgId, string paymentRequestId);

        public Task<MVScbInquiryResult> InquireScbPaymentStatus(string orgId, string paymentRequestId);
        public Task<MVPaymentRequest> RejectPendingPayInRequestById(string orgId, string paymentRequestId, MPaymentRequest pmr);

        public Task<MVBase> VerifyPayInSlipToken(string paymentRequestId, string token);
        public Task<MVBase> UploadPayInSlipById(string paymentRequestId, string token, string base64Image, string? first4 = null, string? last4 = null, string? note = null);
        public List<MDuplicateRecord> CheckPayInSlipDup(string first4, string last4, string? excludeDocumentId = null);
        public Task<MVPayInSlipUploads> GetPayInSlipUploads(string orgId, string paymentRequestId);
        public Task<MVBase> GeneratePayInSlipUploadToken(string orgId, string paymentRequestId);
        public Task<MVPayOutSlipUploads> GetPayOutSlipUploads(string orgId, string paymentRequestId);
        public Task<MVBase> GeneratePayOutSlipUploadToken(string orgId, string paymentRequestId);
    }
}
