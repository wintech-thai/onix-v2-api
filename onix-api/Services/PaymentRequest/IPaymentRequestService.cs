using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPaymentRequestService
    {
        public Task<MVPaymentRequest> GetPaymentRequestById(string orgId, string paymentRequestId);

        public Task<MVPaymentResponse> AddPaymentRequestPayIn(string orgId, MPaymentRequest paymentRequest, MMerchant merchant);
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
    }
}
