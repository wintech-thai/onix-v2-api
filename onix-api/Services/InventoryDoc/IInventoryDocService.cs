using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IInventoryDocService
    {
        public Task<MVInventoryDoc> AddInventoryDocStockIn(string orgId, MInventoryDoc doc);
        public MVInventoryDoc UpdateInventoryDocStockIn(string orgId, string inventoryDocId, MInventoryDoc doc);
        public MVInventoryDoc ApproveInventoryDocStockIn(string orgId, string inventoryDocId);
        public MVInventoryDoc CancelInventoryDocStockIn(string orgId, string inventoryDocId);

        public Task<MVInventoryDoc> AddInventoryDocStockOut(string orgId, MInventoryDoc doc);
        public MVInventoryDoc UpdateInventoryDocStockOut(string orgId, string inventoryDocId, MInventoryDoc doc);
        public MVInventoryDoc ApproveInventoryDocStockOut(string orgId, string inventoryDocId);
        public MVInventoryDoc CancelInventoryDocStockOut(string orgId, string inventoryDocId);

        public Task<MVInventoryDoc> AddInventoryDocTransfer(string orgId, MInventoryDoc doc);
        public MVInventoryDoc UpdateInventoryDocTransfer(string orgId, string inventoryDocId, MInventoryDoc doc);
        public MVInventoryDoc ApproveInventoryDocTransfer(string orgId, string inventoryDocId);
        public MVInventoryDoc CancelInventoryDocTransfer(string orgId, string inventoryDocId);

        public MInventoryDoc? GetInventoryDocById(string orgId, string inventoryDocId);
        public IEnumerable<MInventoryDoc> GetInventoryDocs(string orgId, VMInventoryDoc param);
        public int GetInventoryDocCount(string orgId, VMInventoryDoc param);
    }
}
