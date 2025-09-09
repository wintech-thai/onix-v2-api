using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemTemplateRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MScanItemTemplate AddScanItemTemplate(MScanItemTemplate action);
        public int GetScanItemTemplateCount(VMScanItemTemplate param);
        public MScanItemTemplate GetScanItemTemplateById(string actionId);
        public MScanItemTemplate GetScanItemTemplate();
        public MScanItemTemplate? DeleteScanItemTemplateById(string actionId);
        public MScanItemTemplate? UpdateScanItemTemplateById(string action, MScanItemTemplate scanItemTemplate);
    }
}
