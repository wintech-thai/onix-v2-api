using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPaymentDocumentRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<bool> IsPaymentRequestIdExist(string paymentDocumentId);
        public Task<List<MPaymentDocument>> GetPaymentDocuments(VMPaymentDocument param);
        public Task<int> GetPaymentDocumentCount(VMPaymentDocument param);
        public Task<MPaymentDocument?> GetPaymentDocumentById(string paymentDocId);
        public Task<MPaymentDocument> AddPaymentDocument(MPaymentDocument paymentDoc);
        public Task<MPaymentDocument?> UpdatePaymentDocumentById(string paymentDocId, MPaymentDocument paymentDoc);
    }
}
