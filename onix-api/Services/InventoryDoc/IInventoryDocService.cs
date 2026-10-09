using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IInventoryDocService
    {
        public Task<MVInventoryDoc> AddInventoryDocStockIn(string orgId, MInventoryDoc doc);
        public Task<MVInventoryDoc> UpdateInventoryDocStockIn(string orgId, string inventoryDocId, MInventoryDoc doc);
        public Task<MVInventoryDoc> ApproveInventoryDocStockIn(string orgId, string inventoryDocId);
        public Task<MVInventoryDoc> CancelInventoryDocStockIn(string orgId, string inventoryDocId);

        public Task<MVInventoryDoc> AddInventoryDocStockOut(string orgId, MInventoryDoc doc);
        public Task<MVInventoryDoc> UpdateInventoryDocStockOut(string orgId, string inventoryDocId, MInventoryDoc doc);
        public Task<MVInventoryDoc> ApproveInventoryDocStockOut(string orgId, string inventoryDocId);
        public Task<MVInventoryDoc> CancelInventoryDocStockOut(string orgId, string inventoryDocId);

        public Task<MVInventoryDoc> AddInventoryDocTransfer(string orgId, MInventoryDoc doc);
        public Task<MVInventoryDoc> UpdateInventoryDocTransfer(string orgId, string inventoryDocId, MInventoryDoc doc);
        public Task<MVInventoryDoc> ApproveInventoryDocTransfer(string orgId, string inventoryDocId);
        public Task<MVInventoryDoc> CancelInventoryDocTransfer(string orgId, string inventoryDocId);

        public Task<MInventoryDoc?> GetInventoryDocById(string orgId, string inventoryDocId);
        public Task<IEnumerable<MInventoryDoc>> GetInventoryDocs(string orgId, VMInventoryDoc param);
        public Task<int> GetInventoryDocCount(string orgId, VMInventoryDoc param);
    }
}
