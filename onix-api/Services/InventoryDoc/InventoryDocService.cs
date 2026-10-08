using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    // Fully async (matching the repository below) — per team convention, apply the same
    // pattern to any new API work rather than mixing sync/async in the same call chain.
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

            var result = await repository.AddInventoryDocStockIn(doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> UpdateInventoryDocStockIn(string orgId, string inventoryDocId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var result = await repository.UpdateInventoryDocStockIn(inventoryDocId, doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> ApproveInventoryDocStockIn(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = await repository.ApproveInventoryDocStockIn(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> CancelInventoryDocStockIn(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = await repository.CancelInventoryDocStockIn(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> AddInventoryDocStockOut(string orgId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            doc.DocumentNo = await documentNumberService.GetDocumentNumber(orgId, "InventoryExport");

            var result = await repository.AddInventoryDocStockOut(doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> UpdateInventoryDocStockOut(string orgId, string inventoryDocId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var result = await repository.UpdateInventoryDocStockOut(inventoryDocId, doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        // Unlike Stock-In/Transfer, item unit price/amount are never taken from the request —
        // the spec requires them to be pulled from the item's current stock cost at approve
        // time instead. There's no weighted-average stock ledger implemented yet (Stock-In's
        // own approve doesn't maintain one either), so the best available "current cost" is
        // the item master's own Price field — the same field Stock-In's UI already uses to
        // prefill unit price, so this stays consistent with existing behavior.
        public async Task<MVInventoryDoc> ApproveInventoryDocStockOut(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            itemRepository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var doc = await repository.GetInventoryDocById(inventoryDocId);
            var itemUnitPrices = new Dictionary<Guid, decimal>();
            foreach (var item in doc?.Items ?? new List<MInventoryDocItem>())
            {
                if (!item.Id.HasValue || string.IsNullOrEmpty(item.ItemId)) continue;
                // IInventoryItemRepository is not async yet (out of scope for this change) —
                // fine to call synchronously from here, just not end-to-end async for this one hop.
                var masterItem = itemRepository.GetInventoryItemById(item.ItemId);
                itemUnitPrices[item.Id.Value] = masterItem?.Price ?? 0;
            }

            var result = await repository.ApproveInventoryDocStockOut(inventoryDocId, itemUnitPrices);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> CancelInventoryDocStockOut(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = await repository.CancelInventoryDocStockOut(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> AddInventoryDocTransfer(string orgId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var sameLocation = ValidateDifferentLocationsOrNull(doc);
            if (sameLocation != null) return sameLocation;

            doc.DocumentNo = await documentNumberService.GetDocumentNumber(orgId, "InventoryTransfer");

            var result = await repository.AddInventoryDocTransfer(doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> UpdateInventoryDocTransfer(string orgId, string inventoryDocId, MInventoryDoc doc)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var sameLocation = ValidateDifferentLocationsOrNull(doc);
            if (sameLocation != null) return sameLocation;

            var result = await repository.UpdateInventoryDocTransfer(inventoryDocId, doc);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        // Server-side mirror of the frontend's same-location check — never trust the client
        // alone for a rule the spec calls out explicitly ("source and destination must differ").
        // Pure validation, no I/O — stays sync.
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

        public async Task<MVInventoryDoc> ApproveInventoryDocTransfer(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            itemRepository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var doc = await repository.GetInventoryDocById(inventoryDocId);
            var itemUnitPrices = new Dictionary<Guid, decimal>();
            foreach (var item in doc?.Items ?? new List<MInventoryDocItem>())
            {
                if (!item.Id.HasValue || string.IsNullOrEmpty(item.ItemId)) continue;
                var masterItem = itemRepository.GetInventoryItemById(item.ItemId);
                itemUnitPrices[item.Id.Value] = masterItem?.Price ?? 0;
            }

            var result = await repository.ApproveInventoryDocTransfer(inventoryDocId, itemUnitPrices);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        public async Task<MVInventoryDoc> CancelInventoryDocTransfer(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(inventoryDocId, "");
            if (failure != null) return failure;

            var result = await repository.CancelInventoryDocTransfer(inventoryDocId);
            return new MVInventoryDoc { Status = "OK", Description = "Success", InventoryDoc = result };
        }

        // Shared by Update/Approve/Cancel — all three require the doc to still be Pending.
        // Calls the repository's plain check/get methods (never returns an MV from the
        // repository itself) and builds the failure response here at the service layer.
        private async Task<MVInventoryDoc?> ValidatePendingOrNull(string inventoryDocId, string statusMessageSuffix)
        {
            var existing = await repository.GetInventoryDocById(inventoryDocId);
            if (existing == null)
            {
                return new MVInventoryDoc
                {
                    Status = "NOTFOUND",
                    Description = $"Inventory Document ID [{inventoryDocId}] not found",
                };
            }

            if (!await repository.IsInventoryDocPending(inventoryDocId))
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

        public async Task<MInventoryDoc?> GetInventoryDocById(string orgId, string inventoryDocId)
        {
            repository.SetCustomOrgId(orgId);
            return await repository.GetInventoryDocById(inventoryDocId);
        }

        public async Task<IEnumerable<MInventoryDoc>> GetInventoryDocs(string orgId, VMInventoryDoc param)
        {
            repository.SetCustomOrgId(orgId);
            return await repository.GetInventoryDocs(param);
        }

        public async Task<int> GetInventoryDocCount(string orgId, VMInventoryDoc param)
        {
            repository.SetCustomOrgId(orgId);
            return await repository.GetInventoryDocCount(param);
        }
    }
}
