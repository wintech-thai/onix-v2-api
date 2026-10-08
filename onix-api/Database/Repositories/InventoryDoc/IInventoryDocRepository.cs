using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IInventoryDocRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MInventoryDoc AddInventoryDocStockIn(MInventoryDoc doc);
        public MInventoryDoc? UpdateInventoryDocStockIn(string inventoryDocId, MInventoryDoc doc);
        public MInventoryDoc? ApproveInventoryDocStockIn(string inventoryDocId);
        public MInventoryDoc? CancelInventoryDocStockIn(string inventoryDocId);

        public MInventoryDoc AddInventoryDocStockOut(MInventoryDoc doc);
        public MInventoryDoc? UpdateInventoryDocStockOut(string inventoryDocId, MInventoryDoc doc);
        // itemUnitPrices: item row id -> unit price looked up from InventoryItem.Price at approve
        // time (the spec's "pull cost price from stock at approve time") — computed by the
        // service layer (which owns the InventoryItem lookup), applied here in the same save.
        public MInventoryDoc? ApproveInventoryDocStockOut(string inventoryDocId, Dictionary<Guid, decimal> itemUnitPrices);
        public MInventoryDoc? CancelInventoryDocStockOut(string inventoryDocId);

        public MInventoryDoc AddInventoryDocTransfer(MInventoryDoc doc);
        public MInventoryDoc? UpdateInventoryDocTransfer(string inventoryDocId, MInventoryDoc doc);
        // Same as Stock-Out's approve — unit price/amount are never entered manually for a
        // transfer either (per spec), filled in here from the item master's current price.
        public MInventoryDoc? ApproveInventoryDocTransfer(string inventoryDocId, Dictionary<Guid, decimal> itemUnitPrices);
        public MInventoryDoc? CancelInventoryDocTransfer(string inventoryDocId);

        public bool IsInventoryDocPending(string inventoryDocId);
        public MInventoryDoc? GetInventoryDocById(string inventoryDocId);
        public int GetInventoryDocCount(VMInventoryDoc param);
        public IEnumerable<MInventoryDoc> GetInventoryDocs(VMInventoryDoc param);
    }
}
