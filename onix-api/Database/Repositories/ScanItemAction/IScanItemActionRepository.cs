using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemActionRepository
    {
        public void SetCustomOrgId(string customOrgId);

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
