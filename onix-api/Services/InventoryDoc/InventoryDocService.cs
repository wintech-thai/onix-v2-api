using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class InventoryDocService : BaseService, IInventoryDocService
    {
        private readonly IInventoryDocRepository repository;
        private readonly IDocumentNumberService documentNumberService;

        public InventoryDocService(IInventoryDocRepository repo, IDocumentNumberService documentNumberSvc) : base()
        {
            repository = repo;
            documentNumberService = documentNumberSvc;
        }

        public async Task<MVInventoryDoc> AddInventoryDocStockIn(string orgId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            doc.DocumentNo = await documentNumberService.GetDocumentNumber(orgId, "InventoryImport");

            var result = repository.AddInventoryDocStockIn(doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc UpdateInventoryDocStockIn(string orgId, string inventoryDocId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);
            return repository.UpdateInventoryDocStockIn(inventoryDocId, doc);
        }

        public MVInventoryDoc ApproveInventoryDocStockIn(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            return repository.ApproveInventoryDocStockIn(inventoryDocId);
        }

        public MVInventoryDoc CancelInventoryDocStockIn(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            return repository.CancelInventoryDocStockIn(inventoryDocId);
        }

        public MInventoryDoc? GetInventoryDocById(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            return repository.GetInventoryDocById(inventoryDocId);
        }

        public IEnumerable<MInventoryDoc> GetInventoryDocs(string orgId, VMInventoryDoc param)
        {
            repository.SetCustomOrgId(orgId);
            return repository.GetInventoryDocs(param);
        }

        public int GetInventoryDocCount(string orgId, VMInventoryDoc param)
        {
            repository.SetCustomOrgId(orgId);
            return repository.GetInventoryDocCount(param);
        }
    }
}
