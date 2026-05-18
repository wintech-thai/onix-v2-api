using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPaymentTransactionService
    {
        public Task<MVPaymentTransaction> GetPaymentTransactionById(string orgId, string paymentTransactionId);
        public Task<MVPaymentTransaction> ProcessLinePaymentTxNotification(string orgId, string bankAccountId, MPaymentNotiLine paymentNotiLine);
        public Task<List<MPaymentTransaction>> GetPaymentTransactions(string orgId, VMPaymentTransaction param);
        public Task<int> GetPaymentTransactionCount(string orgId, VMPaymentTransaction param);
        public Task<MVPaymentTransaction> UpdatePaymentTransactionById(string orgId, string paymentTransactionId, MPaymentTransaction paymentTransaction);
    }
}
