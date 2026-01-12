using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPointService
    {
        public Task<MVPointTx> AddPoint(string orgId, MPointTx tx);
        public Task<MVPointTx> DeductPoint(string orgId, MPointTx tx);

        public Task<List<MPointTx>> GetPointTxsByWalletId(string orgId, VMPointTx param);
        public Task<int> GetPointTxsCountByWalletId(string orgId, VMPointTx param);

        public Task<MVPointBalance?> GetPointBalanceByWalletId(string orgId, VMPointBalance param);

        public Task<MVWallet> AddWallet(string orgId, MWallet wallet);
        public Task<List<MWallet>> GetWallets(string orgId, VMWallet param);
        public Task<int> GetWalletsCount(string orgId, VMWallet param);
        public Task<MVWallet?> GetWalletById(string orgId, string walletId);
        public Task<MVWallet?> GetWalletByCustomerId(string orgId, string customerId);

        public Task<MVWallet?> UpdateWalletById(string orgId, string walletId, MWallet wallet);
        public Task<MVWallet?> AttachCustomerToWalletById(string orgId, string walletId, string custId);
        public Task<MVWallet?> DeleteWalletById(string orgId, string walletId);
        public MVWallet ValidateResponseData(string orgId, string customerId, MVWallet responseData);
    }
}
