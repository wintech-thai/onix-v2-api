using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IItemService
    {
        public MItem GetItemById(string orgId, string itemId);
        public MVItem? AddItem(string orgId, MItem item);
        public MVItem? DeleteItemById(string orgId, string itemId);
        public IEnumerable<MItem> GetItems(string orgId, VMItem param);
        public IEnumerable<NameValue> GetAllowItemPropertyNames(string orgId);
        public int GetItemCount(string orgId, VMItem param);
        public MVItem? UpdateItemById(string orgId, string itemId, MItem item);
    }
}
