using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPaymentRequestRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<bool> IsRefIdExist(string refId);
        public Task<List<MPaymentRequest>> GetPaymentRequestsForPaymentTx(VMPaymentRequest param);
        public Task<List<MPaymentRequest>> GetPaymentRequests(VMPaymentRequest param);
        public Task<int> GetPaymentRequestCount(VMPaymentRequest param);
        public Task<MPaymentRequest?> GetPaymentRequestById(string paymentRequestId);
        public Task<MPaymentRequest> AddPaymentRequest(MPaymentRequest paymentRequest);
        public Task<MPaymentRequest?> UpdatePaymentRequestById(string paymentRequestId, MPaymentRequest paymentRequest);
        public Task<MPaymentRequest?> UpdatePayOutRequestById(string paymentRequestId, MPaymentRequest paymentRequest);

        public Task<MPaymentRequest?> UpdatePaymentRequestPaidStatusById(string paymentRequestId, string paymentTxId);
        public Task<MPaymentRequest?> UpdatePaymentStatusRejectById(string paymentRequestId, MPaymentRequest paymentRequest);
        public Task<MPaymentRequest?> UpdatePaymentStatusApprovedById(string paymentRequestId, MPaymentRequest paymentRequest);

        public Task<MPaymentRequest?> UpdateTransferRequestById(string paymentRequestId, MPaymentRequest paymentRequest);
        public Task<bool> DeletePayOutRequestById(string paymentRequestId);
    }
}
