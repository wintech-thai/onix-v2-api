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
    }
}
