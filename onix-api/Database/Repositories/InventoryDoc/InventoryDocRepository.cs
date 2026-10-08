using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    // Handles both MInventoryDoc and MInventoryDocItem — one repository backs StockIn,
    // StockOut, and StockTransfer, keyed by DocumentType (set by each Add* method below).
    //
    // Only ever returns plain model classes (never MVInventoryDoc) — matching the
    // convention used by the other Inventory* repositories. Status/description
    // messages for the caller are built at the service layer, which validates by
    // calling the check methods below (e.g. IsInventoryDocPending) before mutating.
    //
    // Fully async (EF Core async LINQ + SaveChangesAsync throughout) — per team convention,
    // all controller/service/repository methods that touch the DB should be async to avoid
    // tying up thread-pool threads under load. Apply the same pattern to any new API work.
    public class InventoryDocRepository : BaseRepository, IInventoryDocRepository
    {
        public InventoryDocRepository(IDataContext ctx)
        {
            context = ctx;
        }

        // Reused by Add (existing items always empty) and Update (real reconciliation) — the
        // API is expected to detect deleted/edited/added rows from the incoming array itself.
        //
        // from/toLocation* here are always the DOCUMENT's own location fields, not whatever
        // the incoming item rows carry (the frontend never sets per-item location — only the
        // document-level From/To Location pickers exist). Every item is stamped with the
        // document's locations so item-level rows are self-contained (matches the comment
        // already on MInventoryDocItem: copied down so callers don't need to join back to the
        // document just to know which location(s) a line item moved).
        private async Task SyncInventoryDocItems(
            Guid documentId, string? documentNo, string? documentType,
            string? fromLocationId, string? fromLocationCode, string? fromLocationName,
            string? toLocationId, string? toLocationCode, string? toLocationName,
            List<MInventoryDocItem>? incomingItems)
        {
            var incoming = incomingItems ?? new List<MInventoryDocItem>();
            var existingItems = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(documentId.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            var incomingIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

            foreach (var existing in existingItems)
            {
                if (!existing.Id.HasValue || !incomingIds.Contains(existing.Id.Value))
                {
                    context!.InventoryDocItems!.Remove(existing);
                }
            }

            foreach (var item in incoming)
            {
                var existing = item.Id.HasValue
                    ? existingItems.FirstOrDefault(e => e.Id == item.Id)
                    : null;

                if (existing != null)
                {
                    existing.Note = item.Note;
                    existing.LotId = item.LotId;
                    existing.Project = item.Project;
                    existing.ToLocationId = toLocationId;
                    existing.ToLocationCode = toLocationCode;
                    existing.ToLocationName = toLocationName;
                    existing.FromLocationId = fromLocationId;
                    existing.FromLocationCode = fromLocationCode;
                    existing.FromLocationName = fromLocationName;
                    existing.ItemId = item.ItemId;
                    existing.ItemCode = item.ItemCode;
                    existing.ItemName = item.ItemName;
                    existing.ItemQuantity = item.ItemQuantity;
                    existing.ItemUnitPrice = item.ItemUnitPrice;
                    existing.ItemAmount = item.ItemAmount;
                }
                else
                {
                    item.Id = Guid.NewGuid();
                    item.OrgId = orgId;
                    item.DocumentId = documentId.ToString();
                    item.DocumentNo = documentNo;
                    item.DocumentType = documentType;
                    item.ToLocationId = toLocationId;
                    item.ToLocationCode = toLocationCode;
                    item.ToLocationName = toLocationName;
                    item.FromLocationId = fromLocationId;
                    item.FromLocationCode = fromLocationCode;
                    item.FromLocationName = fromLocationName;
                    item.CreatedDate = DateTime.UtcNow;
                    await context!.InventoryDocItems!.AddAsync(item);
                }
            }

            await context!.SaveChangesAsync();
        }

        public async Task<MInventoryDoc> AddInventoryDocStockIn(MInventoryDoc doc)
        {
            var items = doc.Items;
            doc.Items = null;

            doc.Id = Guid.NewGuid();
            doc.OrgId = orgId;
            doc.DocumentType = "StockIn";
            doc.DocumentStatus = "Pending";
            doc.CreatedDate = DateTime.UtcNow;
            doc.ApprovedDate = null;
            doc.StatusDate = null;

            await context!.InventoryDocs!.AddAsync(doc);
            await context.SaveChangesAsync();

            await SyncInventoryDocItems(doc.Id!.Value, doc.DocumentNo, doc.DocumentType,
                null, null, null,
                doc.ToLocationId, doc.ToLocationCode, doc.ToLocationName,
                items);

            doc.Items = items;
            return doc;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> UpdateInventoryDocStockIn(string inventoryDocId, MInventoryDoc doc)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.Description = doc.Description;
            existing.ToLocationId = doc.ToLocationId;
            existing.ToLocationCode = doc.ToLocationCode;
            existing.ToLocationName = doc.ToLocationName;
            await context!.SaveChangesAsync();

            await SyncInventoryDocItems(existing.Id!.Value, existing.DocumentNo, existing.DocumentType,
                null, null, null,
                existing.ToLocationId, existing.ToLocationCode, existing.ToLocationName,
                doc.Items);

            existing.Items = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> ApproveInventoryDocStockIn(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            var now = DateTime.UtcNow;
            existing.DocumentStatus = "Approved";
            existing.ApprovedDate = now;
            existing.StatusDate = now;
            await context!.SaveChangesAsync();

            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> CancelInventoryDocStockIn(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.DocumentStatus = "Cancelled";
            existing.StatusDate = DateTime.UtcNow;
            await context!.SaveChangesAsync();

            return existing;
        }

        public async Task<MInventoryDoc> AddInventoryDocStockOut(MInventoryDoc doc)
        {
            var items = doc.Items;
            doc.Items = null;

            doc.Id = Guid.NewGuid();
            doc.OrgId = orgId;
            doc.DocumentType = "StockOut";
            doc.DocumentStatus = "Pending";
            doc.CreatedDate = DateTime.UtcNow;
            doc.ApprovedDate = null;
            doc.StatusDate = null;

            await context!.InventoryDocs!.AddAsync(doc);
            await context.SaveChangesAsync();

            await SyncInventoryDocItems(doc.Id!.Value, doc.DocumentNo, doc.DocumentType,
                doc.FromLocationId, doc.FromLocationCode, doc.FromLocationName,
                null, null, null,
                items);

            doc.Items = items;
            return doc;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> UpdateInventoryDocStockOut(string inventoryDocId, MInventoryDoc doc)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.Description = doc.Description;
            existing.FromLocationId = doc.FromLocationId;
            existing.FromLocationCode = doc.FromLocationCode;
            existing.FromLocationName = doc.FromLocationName;
            await context!.SaveChangesAsync();

            await SyncInventoryDocItems(existing.Id!.Value, existing.DocumentNo, existing.DocumentType,
                existing.FromLocationId, existing.FromLocationCode, existing.FromLocationName,
                null, null, null,
                doc.Items);

            existing.Items = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened. Unlike StockIn's
        // approve, this also fills in each item's unit price/amount (computed by the service
        // from InventoryItem.Price at approve time, per spec — price is never entered manually
        // for Stock-Out).
        public async Task<MInventoryDoc?> ApproveInventoryDocStockOut(string inventoryDocId, Dictionary<Guid, decimal> itemUnitPrices)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            var items = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.Id.HasValue && itemUnitPrices.TryGetValue(item.Id.Value, out var unitPrice))
                {
                    item.ItemUnitPrice = unitPrice;
                    item.ItemAmount = unitPrice * (item.ItemQuantity ?? 0);
                }
            }

            var now = DateTime.UtcNow;
            existing.DocumentStatus = "Approved";
            existing.ApprovedDate = now;
            existing.StatusDate = now;
            await context!.SaveChangesAsync();

            existing.Items = items;
            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> CancelInventoryDocStockOut(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.DocumentStatus = "Cancelled";
            existing.StatusDate = DateTime.UtcNow;
            await context!.SaveChangesAsync();

            return existing;
        }

        public async Task<MInventoryDoc> AddInventoryDocTransfer(MInventoryDoc doc)
        {
            var items = doc.Items;
            doc.Items = null;

            doc.Id = Guid.NewGuid();
            doc.OrgId = orgId;
            doc.DocumentType = "StockTransfer";
            doc.DocumentStatus = "Pending";
            doc.CreatedDate = DateTime.UtcNow;
            doc.ApprovedDate = null;
            doc.StatusDate = null;

            await context!.InventoryDocs!.AddAsync(doc);
            await context.SaveChangesAsync();

            await SyncInventoryDocItems(doc.Id!.Value, doc.DocumentNo, doc.DocumentType,
                doc.FromLocationId, doc.FromLocationCode, doc.FromLocationName,
                doc.ToLocationId, doc.ToLocationCode, doc.ToLocationName,
                items);

            doc.Items = items;
            return doc;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> UpdateInventoryDocTransfer(string inventoryDocId, MInventoryDoc doc)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.Description = doc.Description;
            existing.FromLocationId = doc.FromLocationId;
            existing.FromLocationCode = doc.FromLocationCode;
            existing.FromLocationName = doc.FromLocationName;
            existing.ToLocationId = doc.ToLocationId;
            existing.ToLocationCode = doc.ToLocationCode;
            existing.ToLocationName = doc.ToLocationName;
            await context!.SaveChangesAsync();

            await SyncInventoryDocItems(existing.Id!.Value, existing.DocumentNo, existing.DocumentType,
                existing.FromLocationId, existing.FromLocationCode, existing.FromLocationName,
                existing.ToLocationId, existing.ToLocationCode, existing.ToLocationName,
                doc.Items);

            existing.Items = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened. Same as
        // Stock-Out's approve: item unit price/amount are never entered manually, filled in
        // here from values the service computed via InventoryItem.Price lookup.
        public async Task<MInventoryDoc?> ApproveInventoryDocTransfer(string inventoryDocId, Dictionary<Guid, decimal> itemUnitPrices)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            var items = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.Id.HasValue && itemUnitPrices.TryGetValue(item.Id.Value, out var unitPrice))
                {
                    item.ItemUnitPrice = unitPrice;
                    item.ItemAmount = unitPrice * (item.ItemQuantity ?? 0);
                }
            }

            var now = DateTime.UtcNow;
            existing.DocumentStatus = "Approved";
            existing.ApprovedDate = now;
            existing.StatusDate = now;
            await context!.SaveChangesAsync();

            existing.Items = items;
            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public async Task<MInventoryDoc?> CancelInventoryDocTransfer(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.DocumentStatus = "Cancelled";
            existing.StatusDate = DateTime.UtcNow;
            await context!.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> IsInventoryDocPending(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var doc = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            return doc != null && doc.DocumentStatus == "Pending";
        }

        public async Task<MInventoryDoc?> GetInventoryDocById(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var doc = await context!.InventoryDocs!.FirstOrDefaultAsync(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (doc == null) return null;

            doc.Items = await context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(doc.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToListAsync();

            return doc;
        }

        private ExpressionStarter<MInventoryDoc> InventoryDocPredicate(VMInventoryDoc param)
        {
            var pd = PredicateBuilder.New<MInventoryDoc>();
            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if (!string.IsNullOrEmpty(param.DocumentType))
            {
                pd = pd.And(p => p.DocumentType!.Equals(param.DocumentType));
            }

            if (!string.IsNullOrEmpty(param.DocumentStatus))
            {
                pd = pd.And(p => p.DocumentStatus!.Equals(param.DocumentStatus));
            }

            if (param.FromDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate >= param.FromDate.Value);
            }

            if (param.ToDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate <= param.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var fullTextPd = PredicateBuilder.New<MInventoryDoc>();
                fullTextPd = fullTextPd.Or(p => p.DocumentNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ToLocationName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ToLocationCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.FromLocationName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.FromLocationCode!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<int> GetInventoryDocCount(VMInventoryDoc param)
        {
            var predicate = InventoryDocPredicate(param);
            return await context!.InventoryDocs!.Where(predicate).CountAsync();
        }

        public async Task<IEnumerable<MInventoryDoc>> GetInventoryDocs(VMInventoryDoc param)
        {
            var limit = 0;
            var offset = 0;

            if (param.Offset > 0)
            {
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = InventoryDocPredicate(param);
            var arr = await context!.InventoryDocs!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }
    }
}
