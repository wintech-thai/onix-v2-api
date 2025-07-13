using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IItemRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MItem AddItem(MItem cycle);
        public int GetItemCount(VMItem param);
        public IEnumerable<MItem> GetItems(VMItem param);
        public MItem GetItemById(string cycleId);
        public MItem? DeleteItemById(string cycleId);
        public bool IsItemCodeExist(string cycleName);
        public MItem? UpdateItemById(string cycleId, MItem cycle);
    }
}
