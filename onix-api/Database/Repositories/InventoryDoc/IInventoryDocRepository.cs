using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IInventoryDocRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MInventoryDoc AddInventoryDocStockIn(MInventoryDoc doc);
        public MVInventoryDoc UpdateInventoryDocStockIn(string inventoryDocId, MInventoryDoc doc);
        public MVInventoryDoc ApproveInventoryDocStockIn(string inventoryDocId);
        public MVInventoryDoc CancelInventoryDocStockIn(string inventoryDocId);
        public MInventoryDoc? GetInventoryDocById(string inventoryDocId);
        public int GetInventoryDocCount(VMInventoryDoc param);
        public IEnumerable<MInventoryDoc> GetInventoryDocs(VMInventoryDoc param);
    }
}
