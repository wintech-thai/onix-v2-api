using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPaymentTransactionRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<bool> IsPaymentRequestIdExist(string paymentRequestId);
        public Task<List<MPaymentTransaction>> GetPaymentTransactions(VMPaymentTransaction param);
        public Task<int> GetPaymentTransactionCount(VMPaymentTransaction param);
        public Task<MPaymentTransaction?> GetPaymentTransactionById(string paymentTxId);
        public Task<MPaymentTransaction> AddPaymentTransaction(MPaymentTransaction paymentTx);
        public Task<MPaymentTransaction?> UpdatePaymentTransactionById(string paymentTxId, MPaymentTransaction paymentTx);
    }
}
