using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPointRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MPointBalance?> GetPointBalanceByWalletId(VMPointBalance param);
        public Task<MPointTx> AddPointTxWithBalance(MPointTx tx, MPointBalance currBal, MPointBalance dailyBal);

        public Task<List<MPointTx>> GetPointTxsByWalletId(VMPointTx param);
        public Task<int> GetPointTxsCountByWalletId(VMPointTx param);

        public Task<MPointTx> AddPointTx(MPointTx tx);

        public Task<MWallet> AddWallet(MWallet wallet);
        public Task<MWallet?> UpdateWalletById(string walletId, MWallet wallet);
        public Task<MWallet?> AttachCustomerToWalletById(string walletId, string custId, MEntity customer);
        public Task<MWallet?> GetWalletById(string walletId);
        public Task<MWallet?> DeleteWalletById(string walletId);
        public Task<List<MWallet>> GetWallets(VMWallet param);
        public Task<int> GetWalletsCount(VMWallet param);
    }
}
