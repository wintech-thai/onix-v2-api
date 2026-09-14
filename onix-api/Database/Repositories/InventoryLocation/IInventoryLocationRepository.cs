using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IInventoryLocationRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MInventoryLocation AddInventoryLocation(MInventoryLocation inventoryLocation);
        public int GetInventoryLocationCount(VMInventoryLocation param);
        public IEnumerable<MInventoryLocation> GetInventoryLocations(VMInventoryLocation param);
        public MInventoryLocation GetInventoryLocationById(string inventoryLocationId);
        public MInventoryLocation? DeleteInventoryLocationById(string inventoryLocationId);
        public bool IsInventoryLocationCodeExist(string code);
        public bool IsInventoryLocationNameExist(string name);
        public MInventoryLocation? UpdateInventoryLocationById(string inventoryLocationId, MInventoryLocation inventoryLocation);
    }
}
