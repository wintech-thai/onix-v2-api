using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    // Handles both MInventoryDoc and MInventoryDocItem — same repository will back
    // StockOut/StockTransfer later on, keyed by DocumentType.
    //
    // Only ever returns plain model classes (never MVInventoryDoc) — matching the
    // convention used by the other Inventory* repositories. Status/description
    // messages for the caller are built at the service layer, which validates by
    // calling the check methods below (e.g. IsInventoryDocPending) before mutating.
    public class InventoryDocRepository : BaseRepository, IInventoryDocRepository
    {
        public InventoryDocRepository(IDataContext ctx)
        {
            context = ctx;
        }

        // Reused by Add (existing items always empty) and Update (real reconciliation) — the
        // API is expected to detect deleted/edited/added rows from the incoming array itself.
        private void SyncInventoryDocItems(Guid documentId, string? documentNo, string? documentType, List<MInventoryDocItem>? incomingItems)
        {
            var incoming = incomingItems ?? new List<MInventoryDocItem>();
            var existingItems = context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(documentId.ToString()) && i.OrgId!.Equals(orgId))
                .ToList();

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
                    existing.ToLocationId = item.ToLocationId;
                    existing.ToLocationCode = item.ToLocationCode;
                    existing.ToLocationName = item.ToLocationName;
                    existing.FromLocationId = item.FromLocationId;
                    existing.FromLocationCode = item.FromLocationCode;
                    existing.FromLocationName = item.FromLocationName;
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
                    item.CreatedDate = DateTime.UtcNow;
                    context!.InventoryDocItems!.Add(item);
                }
            }

            context!.SaveChanges();
        }

        public MInventoryDoc AddInventoryDocStockIn(MInventoryDoc doc)
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

            context!.InventoryDocs!.Add(doc);
            context.SaveChanges();

            SyncInventoryDocItems(doc.Id!.Value, doc.DocumentNo, doc.DocumentType, items);

            doc.Items = items;
            return doc;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public MInventoryDoc? UpdateInventoryDocStockIn(string inventoryDocId, MInventoryDoc doc)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = context!.InventoryDocs!.FirstOrDefault(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.Description = doc.Description;
            existing.ToLocationId = doc.ToLocationId;
            existing.ToLocationCode = doc.ToLocationCode;
            existing.ToLocationName = doc.ToLocationName;
            context!.SaveChanges();

            SyncInventoryDocItems(existing.Id!.Value, existing.DocumentNo, existing.DocumentType, doc.Items);

            existing.Items = context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(existing.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToList();

            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public MInventoryDoc? ApproveInventoryDocStockIn(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = context!.InventoryDocs!.FirstOrDefault(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            var now = DateTime.UtcNow;
            existing.DocumentStatus = "Approved";
            existing.ApprovedDate = now;
            existing.StatusDate = now;
            context!.SaveChanges();

            return existing;
        }

        // Callers (the service layer) must check IsInventoryDocPending first — this method
        // mutates unconditionally and assumes that validation already happened.
        public MInventoryDoc? CancelInventoryDocStockIn(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var existing = context!.InventoryDocs!.FirstOrDefault(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (existing == null) return null;

            existing.DocumentStatus = "Cancelled";
            existing.StatusDate = DateTime.UtcNow;
            context!.SaveChanges();

            return existing;
        }

        public bool IsInventoryDocPending(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var doc = context!.InventoryDocs!.FirstOrDefault(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            return doc != null && doc.DocumentStatus == "Pending";
        }

        public MInventoryDoc? GetInventoryDocById(string inventoryDocId)
        {
            var id = Guid.Parse(inventoryDocId);
            var doc = context!.InventoryDocs!.FirstOrDefault(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id));
            if (doc == null) return null;

            doc.Items = context!.InventoryDocItems!
                .Where(i => i.DocumentId!.Equals(doc.Id.ToString()) && i.OrgId!.Equals(orgId))
                .ToList();

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

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public int GetInventoryDocCount(VMInventoryDoc param)
        {
            var predicate = InventoryDocPredicate(param);
            return context!.InventoryDocs!.Where(predicate).Count();
        }

        public IEnumerable<MInventoryDoc> GetInventoryDocs(VMInventoryDoc param)
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
            var arr = context!.InventoryDocs!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }
    }
}
