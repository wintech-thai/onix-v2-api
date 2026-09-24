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
        public bool IsInventoryDocPending(string inventoryDocId);
        public MInventoryDoc? GetInventoryDocById(string inventoryDocId);
        public int GetInventoryDocCount(VMInventoryDoc param);
        public IEnumerable<MInventoryDoc> GetInventoryDocs(VMInventoryDoc param);
    }
}
