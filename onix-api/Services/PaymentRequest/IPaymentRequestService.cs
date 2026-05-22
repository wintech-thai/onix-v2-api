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
        public Task<List<MPaymentRequest>> GetPaymentRequests(string orgId, VMPaymentRequest param);
        public Task<int> GetPaymentRequestCount(string orgId, VMPaymentRequest param);
        public Task<MVPaymentRequest> UpdatePaymentRequestById(string orgId, string paymentRequestId, MPaymentRequest paymentRequest);
    }
}
