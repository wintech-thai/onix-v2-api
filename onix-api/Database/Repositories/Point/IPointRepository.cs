using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPointRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MPointBalance?> GetPointBalanceByWalletId(VMPointBalance param);
        public Task<MPointTx> AddPointTxWithBalance(MPointTx tx, MPointBalance currBal, MPointBalance dailyBal);
        public Task<MPointTx> AddPointTxWithBalanceV2(MPointTx tx, MPointBalance currBal, MPointBalance dailyBal, decimal quantityBalance, decimal averagePrice);

        public Task<List<MPointTx>> GetPointTxsByWalletId(VMPointTx param);
        public Task<int> GetPointTxsCountByWalletId(VMPointTx param);

        public Task<MPointTx> AddPointTx(MPointTx tx);

        public Task<MWallet> AddWallet(MWallet wallet);
        public Task<MWallet?> UpdateWalletById(string walletId, MWallet wallet);
        public Task<MWallet?> AttachCustomerToWalletById(string walletId, string custId, MEntity customer);
        public Task<MWallet?> GetWalletById(string walletId);
        public Task<MWallet?> GetWalletByCustomerId(string customerId);
        public Task<MWallet?> GetWalletByRefId(string refId);
        public Task<MWallet?> GetWalletByRefIdAndType(string refId, string walletType);
        public Task<MWallet?> GetWalletByLocationItemId(string locationId, string itemId);
        public Task<List<MWallet>> GetWalletsByLocationId(string locationId);
        public Task<List<MWallet>> GetWalletsByItemId(string itemId);
        public Task<MWallet?> GetWalletByMerchantId(string merchantId);
        public Task<MWallet?> GetWalletByBankAccountId(string bankAccountId);
        public Task<MWallet?> DeleteWalletById(string walletId);
        public Task<List<MWallet>> GetWallets(VMWallet param);
        public Task<List<MWallet>> GetWalletBalancesGroupByMerchantId();
        public Task<List<MWallet>> GetPointWalletBalancesGroupByRefId(string walletType);
        public Task<List<MWallet>> GetWalletBalancesGroupByBankAccountId();
        public Task<int> GetWalletsCount(VMWallet param);
    }
}
