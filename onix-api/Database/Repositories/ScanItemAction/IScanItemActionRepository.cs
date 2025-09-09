using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemActionRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MScanItemAction AddScanItemAction(MScanItemAction action);
        public int GetScanItemActionCount(VMScanItemAction param);
        public MScanItemAction GetScanItemActionById(string actionId);
        public MScanItemAction GetScanItemAction();
        public MScanItemAction? DeleteScanItemActionById(string actionId);
        public MScanItemAction? UpdateScanItemActionById(string action, MScanItemAction scanItemAction);
    }
}
