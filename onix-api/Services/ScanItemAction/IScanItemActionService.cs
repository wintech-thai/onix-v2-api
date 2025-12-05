using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemActionService
    {
        public MScanItemAction GetScanItemActionById(string orgId, string actionId);
        public MScanItemAction GetScanItemAction(string orgId);
        public MScanItemAction GetScanItemActionDefault(string orgId);
        public MVScanItemAction? AddScanItemAction(string orgId, MScanItemAction action);
        public MVScanItemAction? DeleteScanItemActionById(string orgId, string actionId);
        public int GetScanItemActionCount(string orgId, VMScanItemAction param);
        public MVScanItemAction? UpdateScanItemActionById(string orgId, string actionId, MScanItemAction action);


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
