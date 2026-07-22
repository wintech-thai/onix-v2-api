using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPaymentDocumentService
    {
        public Task<MVPaymentDocument> GetPaymentDocumentById(string orgId, string paymentDocumentId);
        public Task<List<MPaymentDocument>> GetPaymentDocuments(string orgId, VMPaymentDocument param);
        public Task<int> GetPaymentDocumentCount(string orgId, VMPaymentDocument param);
        public Task<MVPaymentDocument> UpdatePaymentDocumentById(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument);
        public Task<MVPaymentDocument> ApprovePaymentDocumentById(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument);
        public Task<MVPaymentDocument> ApprovePaymentDocumentById_V2(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument);
        public Task<MVPaymentDocument> RejectPaymentDocumentById(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument);
        public Task<MVPaymentDocument> AddPaymentDocument(string orgId, MPaymentDocument paymentDocument);
        public Task<MVPresignedUrl> GetPayInSlipUploadPresignedUrl(string orgId, MMerchant merchant, VMUploadDocument param);
    }
}
