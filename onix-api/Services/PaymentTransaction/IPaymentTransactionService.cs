using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPaymentTransactionService
    {
        public Task<MVPaymentTransaction> GetPaymentTransactionById(string orgId, string paymentTransactionId);
        public Task<MVPaymentTransaction> ProcessLinePaymentTxNotification(string orgId, string bankAccountId, MPaymentNotiLine paymentNotiLine);
        
        public Task<MVPaymentTransaction> ApproveUnidentifiedPaymentTx(string orgId, string paymentTransactionId, string merchantId);
        public Task<MVPaymentTransaction> RejectUnidentifiedPaymentTx(string orgId, string paymentTransactionId, MPaymentTransaction pmt);

        public Task<List<MPaymentTransaction>> GetPaymentTransactions(string orgId, VMPaymentTransaction param);
        public Task<List<MPaymentRequest>> GetPaymentRequestsForPaymentTx(string orgId, VMPaymentRequest param);
        public Task<int> GetPaymentTransactionCount(string orgId, VMPaymentTransaction param);
        public Task<MVPaymentTransaction> UpdatePaymentTransactionById(string orgId, string paymentTransactionId, MPaymentTransaction paymentTransaction);
    }
}
