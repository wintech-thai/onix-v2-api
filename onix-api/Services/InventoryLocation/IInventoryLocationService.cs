using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IInventoryLocationService
    {
        public MInventoryLocation GetInventoryLocationById(string orgId, string inventoryLocationId);
        public MVInventoryLocation? AddInventoryLocation(string orgId, MInventoryLocation inventoryLocation);
        public MVInventoryLocation? DeleteInventoryLocationById(string orgId, string inventoryLocationId);
        public IEnumerable<MInventoryLocation> GetInventoryLocations(string orgId, VMInventoryLocation param);
        public int GetInventoryLocationCount(string orgId, VMInventoryLocation param);
        public MVInventoryLocation? UpdateInventoryLocationById(string orgId, string inventoryLocationId, MInventoryLocation inventoryLocation);
    }
}
