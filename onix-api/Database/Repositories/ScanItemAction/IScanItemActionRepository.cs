using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemActionRepository
    {
        public void SetCustomOrgId(string customOrgId);


        public MScanItemAction AddScanItemAction(MScanItemAction action);
        public MScanItemAction GetScanItemActionById(string actionId);
        public MScanItemAction GetScanItemAction();
        public int GetScanItemActionCount(VMScanItemAction param);
        public MScanItemAction? DeleteScanItemActionById(string actionId);
        public MScanItemAction? UpdateScanItemActionById(string action, MScanItemAction scanItemAction);


        public Task<bool> IsScanItemActionExist(string actionName);
        public Task<List<MScanItemAction>> GetScanItemActions_V2(VMScanItemAction param);
        public Task<MScanItemAction?> GetDefaultScanItemAction_V2();
        public Task<int> GetScanItemActionsCount_V2(VMScanItemAction param);
        public Task<MScanItemAction?> GetScanItemActionById_V2(string actionId);
        public Task<MScanItemAction> AddScanItemAction_V2(MScanItemAction action);
        public Task<MScanItemAction?> DeleteScanItemActionById_V2(string actionId);
        public Task<MScanItemAction?> UpdateScanItemActionById_V2(string actionId, MScanItemAction scanItemAction);
        public Task<MScanItemAction?> SetScanItemActionDefault_V2(string actionId);
    }
}
