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
        private readonly IInventoryItemRepository itemRepository;

        public InventoryDocService(IInventoryDocRepository repo, IDocumentNumberService documentNumberSvc, IInventoryItemRepository itemRepo) : base()
        {
            repository = repo;
            documentNumberService = documentNumberSvc;
            itemRepository = itemRepo;
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

        public async Task<MVInventoryDoc> AddInventoryDocStockOut(string orgId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            doc.DocumentNo = await documentNumberService.GetDocumentNumber(orgId, "InventoryExport");

            var result = repository.AddInventoryDocStockOut(doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc UpdateInventoryDocStockOut(string orgId, string inventoryDocId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var result = repository.UpdateInventoryDocStockOut(inventoryDocId, doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        // Unlike Stock-In/Transfer, item unit price/amount are never taken from the request —
        // the spec requires them to be pulled from the item's current stock cost at approve
        // time instead. There's no weighted-average stock ledger implemented yet (Stock-In's
        // own approve doesn't maintain one either), so the best available "current cost" is
        // the item master's own Price field — the same field Stock-In's UI already uses to
        // prefill unit price, so this stays consistent with existing behavior.
        public MVInventoryDoc ApproveInventoryDocStockOut(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            itemRepository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var doc = repository.GetInventoryDocById(inventoryDocId);
            var itemUnitPrices = new Dictionary<Guid, decimal>();
            foreach (var item in doc?.Items ?? new List<MInventoryDocItem>())
            {
                if (!item.Id.HasValue || string.IsNullOrEmpty(item.ItemId)) continue;
                var masterItem = itemRepository.GetInventoryItemById(item.ItemId);
                itemUnitPrices[item.Id.Value] = masterItem?.Price ?? 0;
            }

            var result = repository.ApproveInventoryDocStockOut(inventoryDocId, itemUnitPrices);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc CancelInventoryDocStockOut(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = repository.CancelInventoryDocStockOut(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> AddInventoryDocTransfer(string orgId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var sameLocation = ValidateDifferentLocationsOrNull(doc);
            if (sameLocation != null) return sameLocation;

            doc.DocumentNo = await documentNumberService.GetDocumentNumber(orgId, "InventoryTransfer");

            var result = repository.AddInventoryDocTransfer(doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc UpdateInventoryDocTransfer(string orgId, string inventoryDocId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var sameLocation = ValidateDifferentLocationsOrNull(doc);
            if (sameLocation != null) return sameLocation;

            var result = repository.UpdateInventoryDocTransfer(inventoryDocId, doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        // Server-side mirror of the frontend's same-location check — never trust the client
        // alone for a rule the spec calls out explicitly ("source and destination must differ").
        private static MVInventoryDoc? ValidateDifferentLocationsOrNull(MInventoryDoc doc)
        {
            if (!string.IsNullOrEmpty(doc.FromLocationId) && doc.FromLocationId == doc.ToLocationId)
            {
                return new MVInventoryDoc
                {
                    Status = "INVALID_REQUEST",
                    Description = "Source and destination location must not be the same",
                };
            }

            return null;
        }

        public MVInventoryDoc ApproveInventoryDocTransfer(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = repository.ApproveInventoryDocTransfer(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public MVInventoryDoc CancelInventoryDocTransfer(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = repository.CancelInventoryDocTransfer(inventoryDocId);
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
