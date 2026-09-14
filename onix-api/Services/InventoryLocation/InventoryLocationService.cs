using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class InventoryLocationService : BaseService, IInventoryLocationService
    {
        private readonly IInventoryLocationRepository? repository = null;

        public InventoryLocationService(IInventoryLocationRepository repo) : base()
        {
            repository = repo;
        }

        public MInventoryLocation GetInventoryLocationById(string orgId, string inventoryLocationId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetInventoryLocationById(inventoryLocationId);

            return result;
        }

        public MVInventoryLocation? AddInventoryLocation(string orgId, MInventoryLocation inventoryLocation)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVInventoryLocation();

            if (repository!.IsInventoryLocationCodeExist(inventoryLocation.Code!))
            {
                r.Status = "DUPLICATE";
                r.Description = $"Inventory Location code [{inventoryLocation.Code}] is duplicate";

                return r;
            }

            if (repository!.IsInventoryLocationNameExist(inventoryLocation.Name!))
            {
                r.Status = "DUPLICATE";
                r.Description = $"Inventory Location name [{inventoryLocation.Name}] is duplicate";

                return r;
            }

            var result = repository!.AddInventoryLocation(inventoryLocation);

            r.Status = "OK";
            r.Description = "Success";
            r.InventoryLocation = result;

            return r;
        }

        public MVInventoryLocation? UpdateInventoryLocationById(string orgId, string inventoryLocationId, MInventoryLocation inventoryLocation)
        {
            var r = new MVInventoryLocation()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateInventoryLocationById(inventoryLocationId, inventoryLocation);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Inventory Location ID [{inventoryLocationId}] not found for the organization [{orgId}]";

                return r;
            }

            r.InventoryLocation = result;
            return r;
        }

        public MVInventoryLocation? DeleteInventoryLocationById(string orgId, string inventoryLocationId)
        {
            var r = new MVInventoryLocation()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(inventoryLocationId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Inventory Location ID [{inventoryLocationId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteInventoryLocationById(inventoryLocationId);

            r.InventoryLocation = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Inventory Location ID [{inventoryLocationId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MInventoryLocation> GetInventoryLocations(string orgId, VMInventoryLocation param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetInventoryLocations(param);

            return result;
        }

        public int GetInventoryLocationCount(string orgId, VMInventoryLocation param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetInventoryLocationCount(param);

            return result;
        }
    }
}
