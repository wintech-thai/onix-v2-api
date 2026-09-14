using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class InventoryLocationRepository : BaseRepository, IInventoryLocationRepository
    {
        public InventoryLocationRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MInventoryLocation AddInventoryLocation(MInventoryLocation inventoryLocation)
        {
            inventoryLocation.Id = Guid.NewGuid();
            inventoryLocation.CreatedDate = DateTime.UtcNow;
            inventoryLocation.UpdatedDate = DateTime.UtcNow;
            inventoryLocation.OrgId = orgId;

            context!.InventoryLocations!.Add(inventoryLocation);
            context.SaveChanges();

            return inventoryLocation;
        }

        private ExpressionStarter<MInventoryLocation> InventoryLocationPredicate(VMInventoryLocation param)
        {
            var pd = PredicateBuilder.New<MInventoryLocation>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MInventoryLocation>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.LocationType != null) && (param.LocationType != ""))
            {
                var typePd = PredicateBuilder.New<MInventoryLocation>();
                typePd = typePd.Or(p => p.LocationType!.Equals(param.LocationType));

                pd = pd.And(typePd);
            }

            return pd;
        }

        public int GetInventoryLocationCount(VMInventoryLocation param)
        {
            var predicate = InventoryLocationPredicate(param);
            var cnt = context!.InventoryLocations!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MInventoryLocation> GetInventoryLocations(VMInventoryLocation param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = InventoryLocationPredicate(param!);
            var arr = context!.InventoryLocations!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MInventoryLocation GetInventoryLocationById(string inventoryLocationId)
        {
            Guid id = Guid.Parse(inventoryLocationId);

            var u = context!.InventoryLocations!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsInventoryLocationCodeExist(string code)
        {
            var cnt = context!.InventoryLocations!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public bool IsInventoryLocationNameExist(string name)
        {
            var cnt = context!.InventoryLocations!.Where(p => p!.Name!.Equals(name)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MInventoryLocation? DeleteInventoryLocationById(string inventoryLocationId)
        {
            Guid id = Guid.Parse(inventoryLocationId);

            var r = context!.InventoryLocations!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.InventoryLocations!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MInventoryLocation? UpdateInventoryLocationById(string inventoryLocationId, MInventoryLocation inventoryLocation)
        {
            Guid id = Guid.Parse(inventoryLocationId);
            var result = context!.InventoryLocations!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                //Not allow to update code
                result.Name = inventoryLocation.Name;
                result.LocationType = inventoryLocation.LocationType;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}
