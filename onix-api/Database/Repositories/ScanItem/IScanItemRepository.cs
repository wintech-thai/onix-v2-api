using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public MScanItem RegisterScanItem(string itemId);
        public MScanItem? GetScanItemBySerialPin(string serial, string pin);
    }
}
