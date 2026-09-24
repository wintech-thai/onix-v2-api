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

            var failure = ValidatePendingOrNull(inventoryDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var result = repository.UpdateInventoryDocStockIn(inventoryDocId, doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc ApproveInventoryDocStockIn(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = repository.ApproveInventoryDocStockIn(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc CancelInventoryDocStockIn(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = repository.CancelInventoryDocStockIn(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        // Shared by Update/Approve/Cancel — all three require the doc to still be Pending.
        // Calls the repository's plain check/get methods (never returns an MV from the
        // repository itself) and builds the failure response here at the service layer.
        private MVInventoryDoc? ValidatePendingOrNull(string inventoryDocId, string statusMessageSuffix)
        {
            var existing = repository.GetInventoryDocById(inventoryDocId);
            if (existing == null)
            {
                return new MVInventoryDoc
                {
                    Status = "NOTFOUND",
                    Description = $"Inventory Document ID [{inventoryDocId}] not found",
                };
            }

            if (!repository.IsInventoryDocPending(inventoryDocId))
            {
                return new MVInventoryDoc
                {
                    Status = "INVALID_STATUS",
                    Description = $"Inventory Document [{existing.DocumentNo}] is already [{existing.DocumentStatus}]{statusMessageSuffix}",
                    InventoryDoc = existing,
                };
            }

            return null;
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
