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
        private readonly IPointService pointService;

        public InventoryDocService(IInventoryDocRepository repo, IDocumentNumberService documentNumberSvc, IInventoryItemRepository itemRepo, IPointService pointSvc) : base()
        {
            repository = repo;
            documentNumberService = documentNumberSvc;
            itemRepository = itemRepo;
            pointService = pointSvc;
        }

        // ตรวจสอบยอดคงเหลือ (quantity) ของทุก item ก่อน approve จริง — กันไม่ให้เอกสาร
        // ถูก approve (flip status) ไปแล้วแต่ posting ไปติดลบค้างอยู่ครึ่งๆ กลางๆ
        private async Task<MVInventoryDoc?> ValidateSufficientStockOrNull(string orgId, string? locationId, List<MInventoryDocItem> items)
        {
            if (string.IsNullOrEmpty(locationId)) return null;

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.ItemId)) continue;
                var requested = item.ItemQuantity ?? 0;
                if (requested <= 0) continue;

                var wallet = await pointService.GetInventoryWalletByLocationItemId(orgId, locationId, item.ItemId);
                var available = wallet?.PointQuantityBalanceDecimal ?? 0;

                if (requested > available)
                {
                    return new MVInventoryDoc
                    {
                        Status = "INSUFFICIENT_STOCK",
                        Description = $"Item [{item.ItemCode}] insufficient stock at location: available [{available}], requested [{requested}]",
                    };
                }
            }

            return null;
        }

        // Posts the wallet-balance side effect (quantity + weighted-average price) for each item
        // of an already-approved doc. Collected failures (should only happen under a race with
        // another concurrent approve — ValidateSufficientStockOrNull already covers the common
        // case) are surfaced as a warning on the response rather than silently dropped, since the
        // document's status has already been committed to Approved by this point.
        private static string? JoinWarnings(List<string> warnings) => warnings.Count == 0 ? null : string.Join(" | ", warnings);

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

            // Items (quantity + unit price/amount ที่กรอกไว้ตอนสร้างเอกสาร) ต้องดึงมาก่อน
            // approve เพราะ repository ของ StockIn ไม่ได้คืน Items กลับมาให้
            var doc = await repository.GetInventoryDocById(inventoryDocId);
            var items = doc?.Items ?? new List<MInventoryDocItem>();

            var result = await repository.ApproveInventoryDocStockIn(inventoryDocId);

            var warnings = new List<string>();
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.ItemId)) continue;
                var qty = item.ItemQuantity ?? 0;
                if (qty <= 0) continue;

                var addResult = await pointService.AddPointV2(orgId, doc!.ToLocationId!, item.ItemId, qty, item.ItemAmount ?? 0,
                    tags: inventoryDocId, docNo: doc.DocumentNo, description: "StockIn");
                if (addResult.Status != "OK")
                {
                    warnings.Add($"Item [{item.ItemCode}]: {addResult.Description}");
                }
            }

            return new MVInventoryDoc { Status = "OK", Description = JoinWarnings(warnings) ?? "Success", InventoryDoc = result };
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
            var items = doc?.Items ?? new List<MInventoryDocItem>();

            var itemUnitPrices = new Dictionary<Guid, decimal>();
            foreach (var item in items)
            {
                if (!item.Id.HasValue || string.IsNullOrEmpty(item.ItemId)) continue;
                // IInventoryItemRepository is not async yet (out of scope for this change) —
                // fine to call synchronously from here, just not end-to-end async for this one hop.
                var masterItem = itemRepository.GetInventoryItemById(item.ItemId);
                itemUnitPrices[item.Id.Value] = masterItem?.Price ?? 0;
            }

            var stockFailure = await ValidateSufficientStockOrNull(orgId, doc?.FromLocationId, items);
            if (stockFailure != null) return stockFailure;

            var result = await repository.ApproveInventoryDocStockOut(inventoryDocId, itemUnitPrices);

            var warnings = new List<string>();
            foreach (var item in result?.Items ?? new List<MInventoryDocItem>())
            {
                if (string.IsNullOrEmpty(item.ItemId)) continue;
                var qty = item.ItemQuantity ?? 0;
                if (qty <= 0) continue;

                var deductResult = await pointService.DeductPointV2(orgId, doc!.FromLocationId!, item.ItemId, qty,
                    tags: inventoryDocId, docNo: doc.DocumentNo, description: "StockOut");
                if (deductResult.Status != "OK")
                {
                    warnings.Add($"Item [{item.ItemCode}]: {deductResult.Description}");
                }
            }

            return new MVInventoryDoc { Status = "OK", Description = JoinWarnings(warnings) ?? "Success", InventoryDoc = result };
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
            var items = doc?.Items ?? new List<MInventoryDocItem>();

            var itemUnitPrices = new Dictionary<Guid, decimal>();
            foreach (var item in items)
            {
                if (!item.Id.HasValue || string.IsNullOrEmpty(item.ItemId)) continue;
                var masterItem = itemRepository.GetInventoryItemById(item.ItemId);
                itemUnitPrices[item.Id.Value] = masterItem?.Price ?? 0;
            }

            var stockFailure = await ValidateSufficientStockOrNull(orgId, doc?.FromLocationId, items);
            if (stockFailure != null) return stockFailure;

            var result = await repository.ApproveInventoryDocTransfer(inventoryDocId, itemUnitPrices);

            var warnings = new List<string>();
            foreach (var item in result?.Items ?? new List<MInventoryDocItem>())
            {
                if (string.IsNullOrEmpty(item.ItemId)) continue;
                var qty = item.ItemQuantity ?? 0;
                if (qty <= 0) continue;

                // มูลค่าที่ไป Add ปลายทาง = มูลค่าที่เบิกออกจากต้นทางจริง (ราคาเฉลี่ย ณ ตอนเบิก × qty)
                var deductResult = await pointService.DeductPointV2(orgId, doc!.FromLocationId!, item.ItemId, qty,
                    tags: inventoryDocId, docNo: doc.DocumentNo, description: "Transfer-Out");
                if (deductResult.Status != "OK")
                {
                    warnings.Add($"Item [{item.ItemCode}] (out): {deductResult.Description}");
                    continue;
                }

                var deductedAmount = deductResult.PointTx?.TxAmountDecimal ?? 0;
                var addResult = await pointService.AddPointV2(orgId, doc.ToLocationId!, item.ItemId, qty, deductedAmount,
                    tags: inventoryDocId, docNo: doc.DocumentNo, description: "Transfer-In");
                if (addResult.Status != "OK")
                {
                    warnings.Add($"Item [{item.ItemCode}] (in): {addResult.Description}");
                }
            }

            return new MVInventoryDoc { Status = "OK", Description = JoinWarnings(warnings) ?? "Success", InventoryDoc = result };
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
