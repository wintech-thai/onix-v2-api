using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IInventoryDocRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MInventoryDoc> AddInventoryDocStockIn(MInventoryDoc doc);
        public Task<MInventoryDoc?> UpdateInventoryDocStockIn(string inventoryDocId, MInventoryDoc doc);
        public Task<MInventoryDoc?> ApproveInventoryDocStockIn(string inventoryDocId);
        public Task<MInventoryDoc?> CancelInventoryDocStockIn(string inventoryDocId);

        public Task<MInventoryDoc> AddInventoryDocStockOut(MInventoryDoc doc);
        public Task<MInventoryDoc?> UpdateInventoryDocStockOut(string inventoryDocId, MInventoryDoc doc);
        // itemUnitPrices: item row id -> unit price looked up from InventoryItem.Price at approve
        // time (the spec's "pull cost price from stock at approve time") — computed by the
        // service layer (which owns the InventoryItem lookup), applied here in the same save.
        public Task<MInventoryDoc?> ApproveInventoryDocStockOut(string inventoryDocId, Dictionary<Guid, decimal> itemUnitPrices);
        public Task<MInventoryDoc?> CancelInventoryDocStockOut(string inventoryDocId);

        public Task<MInventoryDoc> AddInventoryDocTransfer(MInventoryDoc doc);
        public Task<MInventoryDoc?> UpdateInventoryDocTransfer(string inventoryDocId, MInventoryDoc doc);
        // Same as Stock-Out's approve — unit price/amount are never entered manually for a
        // transfer either (per spec), filled in here from the item master's current price.
        public Task<MInventoryDoc?> ApproveInventoryDocTransfer(string inventoryDocId, Dictionary<Guid, decimal> itemUnitPrices);
        public Task<MInventoryDoc?> CancelInventoryDocTransfer(string inventoryDocId);

        public Task<bool> IsInventoryDocPending(string inventoryDocId);
        public Task<MInventoryDoc?> GetInventoryDocById(string inventoryDocId);
        public Task<int> GetInventoryDocCount(VMInventoryDoc param);
        public Task<IEnumerable<MInventoryDoc>> GetInventoryDocs(VMInventoryDoc param);
    }
}
