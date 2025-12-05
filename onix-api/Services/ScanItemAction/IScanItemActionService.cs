using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemActionService
    {
        public Task<MScanItemAction?> GetScanItemActionById_V2(string orgId, string actionId);
        public Task<MScanItemAction?> GetScanItemAction_V2(string orgId);
        public MScanItemAction GetScanItemActionDefault_V2(string orgId);
        public Task<MVScanItemAction> AddScanItemAction_V2(string orgId, MScanItemAction action);
        public Task<MVScanItemAction> DeleteScanItemActionById_V2(string orgId, string actionId);
        public Task<List<MScanItemAction>> GetScanItemActions_V2(string orgId, VMScanItemAction param);
        public Task<int> GetScanItemActionCount_V2(string orgId, VMScanItemAction param);
        public Task<MVScanItemAction> UpdateScanItemActionById_V2(string orgId, string actionId, MScanItemAction action);
        public Task<MVScanItemAction> SetDefaultScanItemActionById_V2(string orgId, string actionId);

    }
}
