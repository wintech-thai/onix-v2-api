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
    }
}
