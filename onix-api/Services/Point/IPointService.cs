using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPointService
    {
        public Task<MVPointTx> AddPoint(string orgId, MPointTx tx);
        public Task<MVPointTx> DeductPoint(string orgId, MPointTx tx);

        // V2 : สำหรับงาน Inventory โดยเฉพาะ (quantity + ราคาเฉลี่ยถ่วงน้ำหนัก), แยกจาก AddPoint/DeductPoint เดิมที่ Please Payment ใช้อยู่
        public Task<MVPointTx> AddPointV2(string orgId, string locationId, string itemId, decimal quantity, decimal amount, string? tags, string? docNo, string? description);
        public Task<MVPointTx> DeductPointV2(string orgId, string locationId, string itemId, decimal quantity, string? tags, string? docNo, string? description);
        public Task<List<MPointTx>> GetPointTxsByLocationItemId(string orgId, string locationId, string itemId, VMPointTx param);
        public Task<MWallet> GetOrCreateInventoryWallet(string orgId, string locationId, string itemId);
        public Task<MWallet?> GetInventoryWalletByLocationItemId(string orgId, string locationId, string itemId);
        public Task<List<MWallet>> GetWalletsByLocationId(string orgId, string locationId);
        public Task<List<MWallet>> GetWalletsByItemId(string orgId, string itemId);

        public Task<List<MPointTx>> GetPointTxsByWalletId(string orgId, VMPointTx param);
        public Task<int> GetPointTxsCountByWalletId(string orgId, VMPointTx param);

        public Task<MVPointBalance?> GetPointBalanceByWalletId(string orgId, VMPointBalance param);

        public Task<MVWallet> AddWallet(string orgId, MWallet wallet);
        public Task<List<MWallet>> GetWallets(string orgId, VMWallet param);
        public Task<int> GetWalletsCount(string orgId, VMWallet param);
        public Task<MVWallet?> GetWalletById(string orgId, string walletId);
        public Task<MVWallet?> GetWalletByCustomerId(string orgId, string customerId);
        public Task<MVWallet?> GetWalletByMerchantId(string orgId, string merchantId);
        public Task<MVWallet?> GetWalletByBankAccountId(string orgId, string bankAccountId);
        public Task<MVWallet?> GetWalletByRefIdAndType(string orgId, string refId, string walletType);

        public Task<MVWallet?> UpdateWalletById(string orgId, string walletId, MWallet wallet);
        public Task<MVWallet?> AttachCustomerToWalletById(string orgId, string walletId, string custId);
        public Task<MVWallet?> DeleteWalletById(string orgId, string walletId);
        public MVWallet ValidateResponseData(string orgId, string customerId, MVWallet responseData);
        public MVWallet ValidateResponseData(string orgId, string customerId, List<MPointTx> items);
        public Task<List<MPointTx>> GetPointTxsByCustomerId(string orgId, string custId, VMPointTx param);
    }
}
