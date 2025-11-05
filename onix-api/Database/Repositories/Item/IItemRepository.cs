using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IItemRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MItem AddItem(MItem item);
        public int GetItemCount(VMItem param);
        public IEnumerable<MItem> GetItems(VMItem param);
        public MItem GetItemById(string itemId);
        public MItem? DeleteItemById(string itemId);
        public bool IsItemCodeExist(string itemCode);
        public MItem? UpdateItemById(string itemId, MItem item);

        public MItem? ApproveItemById(string itemId);
        public MItem? DisableItemById(string itemId);
        public MItemTx AddItemTxWithBalance(MItemTx tx, MItemBalance currBal);
        public MItemBalance? GetItemBalanceByItemId(VMItemBalance param);
        public List<MItemTx> GetItemTxsByItemId(VMItemTx param);
        public int GetItemTxsCountByItemId(VMItemTx param);
    }
}
